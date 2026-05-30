namespace FunctionalBlog.Application.Roles;

public interface IRoleRepository
{
    ValueTask<IReadOnlyList<Role>> All();

    ValueTask<Role?> FindById(RoleId id);

    ValueTask<Role?> FindByName(string name);

    ValueTask<RoleId> NextId();

    ValueTask Save(Role role);

    ValueTask Delete(RoleId id);
}
