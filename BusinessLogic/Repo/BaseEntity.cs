namespace BusinessLogic.Repo
{
    public abstract class BaseEntity<T> where T : BaseEntity<T>
    {
        public override bool Equals(object obj)
        {
            var fooItem = obj as T;
            var valLeft = GetType().GetProperty("Id").GetValue(this, null);
            var valRight = GetType().GetProperty("Id").GetValue(obj, null);
            return valLeft.Equals(valRight);
        }

        public override int GetHashCode()
        {
            var prime = 31;
            var result = 1;
            var valLeft = GetType().GetProperty("Id").GetValue(this, null);
            result = prime * result + valLeft.GetHashCode();
            return result;
        }
    }
}