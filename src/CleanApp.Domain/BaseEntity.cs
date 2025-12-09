using System;
using System.Collections.Generic;
using System.Text;

namespace CleanApp.Domain
{
    public abstract class BaseEntity
    {
        public string Id { get; private set; }
        public DateTime CreateTime { get; private set; }
        public DateTime UpdateTime { get; set; }
        public bool IsDeleted { get; set; }

        public BaseEntity()
        {
            Id = Ulid.NewUlid().ToString();
            CreateTime = DateTime.UtcNow;
            UpdateTime = CreateTime;
            IsDeleted = false;

        }
    }
}
