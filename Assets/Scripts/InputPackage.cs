namespace DefaultNamespace
{
    public struct InputPackage
    {
        public byte id, input;

        public InputPackage(byte id, byte input)
        {
            this.id = id;
            this.input = input;
        }

        public override string ToString()
        {
            return id + ", " + input;
        }
    }
}