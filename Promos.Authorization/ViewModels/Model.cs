namespace Promos.Authentication.ViewModels;

public interface IErrorModel
{
    string Key { get; }
    string Message { get; }
}

public class Model
{
    public List<IErrorModel> Errors { get; } = new();

    public Model WithError(IErrorModel error)
    {
        Errors.Add(error);
        return this;
    }
}


