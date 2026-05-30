namespace FunctionalBlog.Pipeline;

public delegate ValueTask<T> Effect<TEnv, T>(TEnv env);
