namespace SettingModule.Contracts;
public interface ISettingDefinationProvider 
{
    Task Define(ISettingDefinationContext context);
}
