using GraphQL.Types;
using PaaS.Ticketing.ApiLib.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiGraphQL.Types
{
    public class UserType : ObjectGraphType<User>
    {
        public UserType()
        {
            Field(x => x.UserId, type: typeof(IdGraphType)).Description("Id of the User");
            Field(x => x.Firstname).Description("Attendee's first name");
            Field(x => x.Lastname).Description("Attendee's family name");
            Field(x => x.CreatedOn).Description("When the user has been created in the system");
            Field(x => x.BirthDate).Description("Birth date");
            Field(x => x.Email);
        }
    }
}
