namespace FunctionalBlog;

public delegate ValueTask<T> Effect<T>(Env env);
