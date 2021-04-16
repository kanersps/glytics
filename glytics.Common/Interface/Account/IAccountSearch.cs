namespace glytics.Common.Interface.Account
{
    public interface IAccountSearch
    {
        Models.Account GetById(string id);
        Models.Account GetByUsername(string username);
        Models.Account GetByEmail(string email);
        Models.Account GetWithApplications(Models.Account account);
    }
}