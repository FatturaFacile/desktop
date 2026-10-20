namespace FatturaFacile.Application.Common.Interfaces;

public interface IPasswordHasher
{
    public void CreateHash(string password, out string hash, out string salt);
    public bool VerifyHash(string password, string hash, string salt);
}
