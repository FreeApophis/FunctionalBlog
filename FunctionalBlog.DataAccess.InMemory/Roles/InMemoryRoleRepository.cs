using System.Collections.Concurrent;

namespace FunctionalBlog.DataAccess.Roles;

public sealed class InMemoryRoleRepository : IRoleRepository
{
    private readonly ConcurrentDictionary<int, Role> _roles = new();
    private int _nextId = 1;

    public ValueTask<IReadOnlyList<Role>> All() =>
        ValueTask.FromResult<IReadOnlyList<Role>>(_roles.Values.ToList());

    public ValueTask<Role?> FindById(RoleId id) =>
        ValueTask.FromResult(_roles.TryGetValue(id.Value, out var role) ? role : null);

    public ValueTask<Role?> FindByName(string name) =>
        ValueTask.FromResult(
            _roles.Values.FirstOrDefault(r => r.Name == name));

    public ValueTask<RoleId> NextId() =>
        ValueTask.FromResult(new RoleId(Interlocked.Increment(ref _nextId)));

    public ValueTask Save(Role role)
    {
        _roles[role.Id.Value] = role;
        return ValueTask.CompletedTask;
    }

    public ValueTask Delete(RoleId id)
    {
        _roles.TryRemove(id.Value, out _);
        return ValueTask.CompletedTask;
    }
}
