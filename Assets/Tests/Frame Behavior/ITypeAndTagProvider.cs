using System;

public interface ITypeAndTagProvider<Tag> {
  public object Get(Type type, Tag tag);
  public T Get<T>(Tag tag) => (T)Get(typeof(T), tag);
}