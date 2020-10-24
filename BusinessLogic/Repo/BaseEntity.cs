namespace BusinessLogic.Repo
{
    public abstract class BaseEntity<T> where T : BaseEntity<T>
    {
        public override bool Equals(object obj)
        {
            T fooItem = obj as T;
            var valLeft = GetType().GetProperty("Id").GetValue(this, null);
            var valRight = GetType().GetProperty("Id").GetValue(obj, null);
            return valLeft.Equals(valRight);
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            var valLeft = GetType().GetProperty("Id").GetValue(this, null);
            result = prime * result + valLeft.GetHashCode();
            return result;
        }
    }
}