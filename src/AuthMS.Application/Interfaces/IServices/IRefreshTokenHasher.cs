namespace Application.Interfaces.IServices
{
    public interface IRefreshTokenHasher
    {
        string Hash(string token);
    }
}
