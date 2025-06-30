using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DummyDataDog.Models
{
    [Table("users")]  // Map to PostgreSQL table "users"
    public class Users
    {
        [Key]
        [Column("id")]  // Map to PostgreSQL column 'id'
        public int Id { get; set; }

        [Column("username")]  // Map to PostgreSQL column 'username'
        public string Username { get; set; }

        [Column("password")]  // Map to PostgreSQL column 'password'
        public string Password { get; set; }

        [Column("fullname")]  // Map to PostgreSQL column 'fullname'
        public string FullName { get; set; }
    }
}
