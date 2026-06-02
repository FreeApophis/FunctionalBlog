using System.Collections.Concurrent;

namespace FunctionalBlog.DataAccess.Roles;

public sealed class InMemoryRoleRepository : IRoleRepository
{
    private readonly ConcurrentDictionary<int, Role> _roles = new();
    private int _nextId = 1;

    public ValueTask<IReadOnlyList<Role>> All() =>
        ValueTask.FromResult<IReadOnlyList<Role>>(_roles.Values.ToList());

    public ValueTask<Option<Role>> FindById(RoleId id) =>
        ValueTask.FromResult(_roles.TryGetValue(id.Value, out var role) ? Option.Some(role) : Option<Role>.None);

    public ValueTask<Option<Role>> FindByName(string name)
    {
        var role = _roles.Values.FirstOrDefault(r => r.Name == name);
        return ValueTask.FromResult(role is not null ? Option.Some(role) : Option<Role>.None);
    }

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
