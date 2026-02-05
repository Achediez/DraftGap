// Proyecto de dominio: entidades y lógica de negocio
namespace DraftGapBackend.Domain.Users
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        // ...otros campos relevantes...
    }
}
