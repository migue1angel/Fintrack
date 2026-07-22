
## Documentación de Decisión Arquitectónica (ADR): Estrategia de Validación en el Pipeline de MediatR

## Estado
**Aprobado**

## Contexto
En el desarrollo del módulo de Usuarios para **FinTrack v2**, se requiere centralizar la validación de los datos de entrada (Commands/Queries) antes de que alcancen los manejadores de dependencias (`Handlers`). Se utiliza `FluentValidation` en combinación con los `PipelineBehaviors` de `MediatR` en .NET 10 para interceptar las peticiones de manera transversal. 

El sistema utiliza la librería `ErrorOr<T>` como contrato estándar para el flujo de control de errores de negocio, evitando el uso de excepciones tradicionales. El debate técnico se centró en cómo el `ValidationBehavior` genérico debe reportar los fallos de validación hacia las capas superiores sin romper la consistencia arquitectónica ni penalizar el rendimiento en entornos Serverless (AWS Lambda).

---

## Evolución de las Propuestas y Contraste de Perspectivas

### Perspectiva 1: Enfoque Tradicional basado en Excepciones (Exception-driven)
Es el diseño por defecto donde el behavior genérico ejecuta las reglas de FluentValidation y, si encuentra fallos, interrumpe el flujo lanzando una excepción a nivel de infraestructura.

```csharp
// Flujo tradicional: interrupción abrupta mediante excepciones
public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
{
    var result = await _validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        throw new ValidationException(result.Errors); // ← Lanzamiento de excepción
    }
    return await next();
}

```

* **Cómo se maneja:** Requiere de un Middleware global (`UseExceptionHandler`) en el `Program.cs` que capture `ValidationException`, extraiga los mensajes y devuelva un código HTTP 400.
* **Trade-off:** Es simple de implementar y totalmente genérico, pero el lanzamiento de excepciones (`throw`/`catch`) obliga al CLR a capturar el *Stack Trace*, lo que consume ciclos de CPU e introduce latencia en el *Hot Path* de la aplicación, algo crítico para los tiempos de respuesta y costos de una AWS Lambda.

---

### Perspectiva 2: Propuesta Inicial con Reflection Dinámico (Evitando Excepciones)

Para mitigar el costo de las excepciones, se propuso que el Behavior genérico construyera dinámicamente un objeto `ErrorOr<T>` en tiempo de ejecución si la validación fallaba, devolviendo los errores como datos en lugar de interrupciones de flujo.

```csharp
// Propuesta con Bug de anidamiento detectado
if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ErrorOr<>))
{
    // BUG: Generaba ErrorOr<ErrorOr<TValue>> en lugar de ErrorOr<TValue>
    return (TResponse)(dynamic)ErrorOr<TResponse>.From(errors); 
}
throw new ValidationException(validationResult.Errors);

```

* **Fallo Técnico:** Al evaluar `typeof(TResponse)`, si el handler ya devolvía `ErrorOr<RegisterResult>`, el método `ErrorOr<TResponse>.From()` intentaba encapsular el tipo de respuesta completo, provocando un indeseado anidamiento de tipos en runtime (`ErrorOr<ErrorOr<RegisterResult>>`).
* **Trade-off:** Resolvía el problema de rendimiento, pero introducía una lógica de Reflection compleja, frágil ante refactorizaciones y propensa a fallos en runtime que el compilador no podía advertir.

---

### Perspectiva 3: Contrapropuesta Óptima basada en Restricciones de Tipos (`IErrorOr` Constraint)

La solución definitiva refina la propuesta de Reflection eliminando el dinamismo complejo mediante el uso de restricciones genéricas (`where`) de C# y aprovechando las abstracciones nativas de la librería (`IErrorOr`).

```csharp
using ErrorOr;
using FluentValidation;
using MediatR;

namespace FinTrack.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr // ← Restricción clave: Solo aplica a flujos que retornan ErrorOr
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationBehavior(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (_validator is null) return await next();

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        
        if (validationResult.IsValid) return await next();

        // Conversión limpia de FluentValidation a objetos Error de ErrorOr
        var errors = validationResult.Errors
            .ConvertAll(f => Error.Validation(
                code: f.PropertyName,
                description: f.ErrorMessage));

        // El cast vía (dynamic) funciona de forma segura por la conversión implícita de la librería
        return (TResponse)(dynamic)errors;
    }
}

```

* **Cómo funciona:** La interfaz `IErrorOr` es implementada nativamente por `ErrorOr<T>`. Al añadir el constraint, el contenedor de inversión de control (IoC) y MediatR descartan este behavior para cualquier petición que no implemente dicha interfaz. Al fallar la validación, se mapean los errores y se aprovecha el operador de conversión implícita (`implicit operator ErrorOr<TValue>(List<Error> errors)`) para retornar la respuesta de forma directa y tipada.

---

## Matriz de Comparación y Trade-offs

| Criterio | Perspectiva 1 (Excepciones) | Perspectiva 2 (Reflection) | Perspectiva 3 (Constraint `IErrorOr`) |
| --- | --- | --- | --- |
| **Consistencia de Errores** | Baja (Mezcla excepciones con datos) | Alta (Errores como ciudadanos de 1ra clase) | **Absoluta** (Todo unificado en el flujo de datos) |
| **Rendimiento en AWS Lambda** | Bajo (Penalización por *Stack Trace*) | Alto (Flujo lineal sin excepciones) | **Máximo** (Optimizado por compilador y sin excepciones) |
| **Seguridad de Tipos** | Alta (Tiempo de compilación) | Baja (Riesgo de errores genéricos anidados) | **Alta** (Validado por el sistema de tipos de C#) |
| **Complejidad de Código** | Muy Baja | Alta (Reflection complejo en runtime) | **Baja-Media** (Declarativo mediante genéricos) |

### Trade-off de la Decisión Adoptada

El único compromiso (*trade-off*) asumido con la **Perspectiva 3** es que este behavior queda estrictamente acoplado a los flujos de MediatR que decidan utilizar `ErrorOr<T>` como contrato de respuesta. Si en el futuro existieran endpoints técnicos de infraestructura (como un *Health Check* o recolección de métricas puras) que devuelvan tipos primitivos (`string`, `bool`), **quedarán excluidos de la validación automática de este pipeline**.

Se determina que este comportamiento es deseable y aceptable, ya que delimita formalmente por contrato que la validación automatizada es una regla exclusiva de los casos de uso de negocio del dominio.

## Conclusión y Próximos Pasos

Se implementará la **Perspectiva 3** de forma inmediata en el core del sistema. Esto garantiza consistencia absoluta en el endpoint, ya que tanto los errores de validación como los de conflicto de negocio se procesarán exactamente en el mismo bloque (`.Match()`) de las Minimal APIs, eliminando la necesidad de bloques `try-catch` o middlewares de excepción pesados en el hot path.

```

```