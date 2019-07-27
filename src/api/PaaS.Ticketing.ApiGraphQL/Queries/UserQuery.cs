using GraphQL.Types;
using PaaS.Ticketing.ApiGraphQL.Types;
using PaaS.Ticketing.ApiLib.Repositories;

namespace PaaS.Ticketing.ApiGraphQL.Queries
{
    public class UserQuery : ObjectGraphType
    {
        public UserQuery(IUsersRepository usersRepository)
        {
            Field<ListGraphType<UserType>>(
                "users",
                resolve: context => usersRepository.GetUsersAsync(null)
            );
        }
    }

}
