using GraphQL;
using GraphQL.Types;
using PaaS.Ticketing.ApiGraphQL.Queries;

namespace PaaS.Ticketing.ApiGraphQL.Schemas
{
    public class UserSchema : Schema
    {
        public UserSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<UserQuery>();
        }
    }
}
