using BusinessObjects;
using System;
using System.Collections.Concurrent;

namespace BusinessLogic.Repo
{
    public class AuthRepository : BaseRepository<DBPerson>
    {
        protected ConcurrentDictionary<string, Person> persons;

        public AuthRepository()
        {
            persons = new ConcurrentDictionary<string, Person>();
        }

        public DBPerson RegisterUser(Person userModel)
        {
            if (persons.ContainsKey(userModel.Mail)) throw new Exception($"User {userModel.Mail} already exists!!!");

            DBPerson person = new DBPerson
            {
                Mail = userModel.Mail,
                Credential = userModel.Credential,
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
            var result = GetAll().Find(x =>
                x.Mail.Equals(mail) && CheckCredential(x.Credential, password) && x.Retired == false);
            if (result == null)
                return null;
            var dto = toDTO(result);
            persons[result.Mail] = dto;
            return dto;
        }

        protected bool CheckCredential(string cred, string enteredCred)
        {
            return cred.Equals(enteredCred);
        }

        private Person toDTO(DBPerson result)
        {
            Person person = new Person();
            person.Id = result.Id;
            person.Languageid = result.Languageid;
            person.Mail = result.Mail;
            person.Privilege = result.Privilege;
            person.Regip = result.Regip;
            person.Retired = result.Retired;
            person.Uuid = result.Uuid;
            person.CountryId = result.Country.Id;
            person.Credential = result.Credential;
            person.Activated = result.Activated;
            return person;
        }
    }
}