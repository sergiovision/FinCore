using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BusinessLogic.Repo.Domain;
using BusinessObjects;
using BusinessObjects.BusinessObjects;

namespace BusinessLogic.Repo;

public class AuthRepository : BaseRepository<DBPerson>
{
    private const string HashPrefix = "PBKDF2";
    private const int HashIterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    protected ConcurrentDictionary<string, Person> persons;

    public AuthRepository()
    {
        persons = new ConcurrentDictionary<string, Person>();
    }

    public DBPerson RegisterUser(Person userModel)
    {
        if (persons.ContainsKey(userModel.Mail)) throw new Exception($"User {userModel.Mail} already exists!!!");

        var person = new DBPerson
        {
            Mail = userModel.Mail,
            Credential = HashCredential(userModel.Credential),
            Regip = userModel.Regip,
            Activated = userModel.Activated,
            Uuid = userModel.Uuid,
            Languageid = userModel.Languageid,
            Retired = userModel.Retired
        };
        persons[userModel.Mail] = userModel;
        return Insert(person);
    }

    public Person FindUser(string mail, string password)
    {
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            var result = Session.Query<DBPerson>().FirstOrDefault(x => x.Mail == mail && x.Retired == false);
            if (result == null || !CheckCredential(result.Credential, password))
                return null;
            if (!IsHashed(result.Credential))
            {
                // upgrade a legacy plain text credential on successful login
                using (var Transaction = Session.BeginTransaction())
                {
                    result.Credential = HashCredential(password);
                    Transaction.Commit();
                }
            }

            var dto = toDTO(result);
            persons[result.Mail] = dto;
            return dto;
        }
    }

    protected bool CheckCredential(string cred, string enteredCred)
    {
        if (string.IsNullOrEmpty(cred) || enteredCred == null)
            return false;
        if (!IsHashed(cred))
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(cred),
                Encoding.UTF8.GetBytes(enteredCred));

        var parts = cred.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
            return false;
        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(enteredCred, salt, iterations, HashAlgorithmName.SHA256,
            expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static bool IsHashed(string cred)
    {
        return cred.StartsWith(HashPrefix + "$", StringComparison.Ordinal);
    }

    private static string HashCredential(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, HashIterations, HashAlgorithmName.SHA256, HashSize);
        return $"{HashPrefix}${HashIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private Person toDTO(DBPerson result)
    {
        var person = new Person();
        person.Id = result.Id;
        person.Languageid = result.Languageid;
        person.Mail = result.Mail;
        person.Privilege = result.Privilege;
        person.Regip = result.Regip;
        person.Retired = result.Retired;
        person.Uuid = result.Uuid;
        person.CountryId = result.Country?.Id ?? 0;
        person.Credential = result.Credential;
        person.Activated = result.Activated;
        return person;
    }
}
