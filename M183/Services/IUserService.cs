namespace M183.Services
{
    public interface IUserService
    {
        int GetUserId();
        string GetUsername();
        bool IsAdmin();

    }
}
