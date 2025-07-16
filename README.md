## 📚 Module Layers Explained

- **Domain Layer** (`SampleModule.Domain`):
  - Contains core business entities, value objects, enums, and domain interfaces.
  - Independent of any infrastructure or frameworks.

- **Application Layer** (`SampleModule.Application`):
  - Handles business logic.
  - Contains services, handlers, and application-specific DTOs.
  - Depends on Domain Layer.

- **Infrastructure Layer** (`SampleModule.Infrastructure`):
  - Implements repositories, database access (EF Core DbContext), and external integrations.
  - Depends on Domain Layer.

- **API Layer** (`SampleModule.API` - optional):
  - Defines controllers specific to the module.
  - Typically used when modules expose their own API endpoints.

- **UI Layer** (`SampleModule.UI` - optional):
  - Contains Razor components or UI-related code.
  - Used in Blazor Server projects for dynamic component rendering.

---

## ⚙️ How to Load Modules

1. **Compile Each Module**:
   - Build each module as a class library (DLL).
   - Output should go to the `plugins/` directory inside `LMS.Host/bin/`.

2. **Dynamic Loading**:
   - The host application loads DLLs from the `/plugins` folder at runtime
     - Modules register services using interfaces like `IRegisterModuleServices`.

3. **Service Registration (Example)**:
   Each module should implement service registration like this:

```csharp
public static class SampleModuleServiceCollectionExtensions
{
    public static IServiceCollection AddSampleModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SampleModuleDBContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<ISampleService, SampleService>();

        return services;
    }
}
