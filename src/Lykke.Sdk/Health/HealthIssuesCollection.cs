using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Sdk.Health
{
    /// <summary>
    /// Health issue collection.
    /// </summary>
    [PublicAPI]
    public class HealthIssuesCollection : IReadOnlyCollection<HealthIssue>
    {
        private readonly List<HealthIssue> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthIssuesCollection"/> class.
        /// </summary>
        public HealthIssuesCollection()
        {
            _list = new List<HealthIssue>();
        }

        /// <inheritdoc />
        public int Count => _list.Count;

        /// <summary>
        /// Adds a new issue to the collection
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        public void Add(string type, string value)
        {
            _list.Add(HealthIssue.Create(type, value));
        }

        /// <inheritdoc />
        public IEnumerator<HealthIssue> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
