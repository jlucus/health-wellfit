using System;
using System.ComponentModel.DataAnnotations;

namespace WellFitPlus.Database.Entities {
    public class EntityBase<T> {

        private object _id;

        [Key]
        public T Id {
            get {
                if (_id == null && typeof(T) == typeof(Guid)) {
                    _id = Guid.NewGuid();
                }

                return _id == null ? default(T) : (T)_id;
            }

            set { _id = value; }
        }
    }
}
