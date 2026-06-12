namespace Common.Extensions {
    public static class CollectionExt {
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items) {
            if (source == null)
                throw new ArgumentNullException("source", "source is null.");
            foreach (var item in items)
                source.Add(item);
        }
    }
}
