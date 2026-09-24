namespace EthicalLab.Shared
{
    public readonly struct Result
    {
        public bool Ok { get; }
        public string Error { get; }

        Result(bool ok, string error)
        {
            Ok = ok;
            Error = error;
        }

        public static Result Success() => new Result(true, null);
        public static Result Fail(string error) => new Result(false, error ?? "error");
    }

    public readonly struct Result<T>
    {
        public bool Ok { get; }
        public T Value { get; }
        public string Error { get; }

        Result(bool ok, T value, string error)
        {
            Ok = ok;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);
        public static Result<T> Fail(string error) => new Result<T>(false, default, error ?? "error");
    }

    public readonly struct MissionId
    {
        public string Value { get; }
        public MissionId(string value) { Value = value ?? ""; }
        public override string ToString() => Value;
        public bool Equals(MissionId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is MissionId other && Equals(other);
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
    }

    public readonly struct ConceptId
    {
        public string Value { get; }
        public ConceptId(string value) { Value = value ?? ""; }
        public override string ToString() => Value;
        public bool Equals(ConceptId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ConceptId other && Equals(other);
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
    }

    public readonly struct StepId
    {
        public string Value { get; }
        public StepId(string value) { Value = value ?? ""; }
        public override string ToString() => Value;
        public bool Equals(StepId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is StepId other && Equals(other);
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
        public string ClueGroup
        {
            get
            {
                int dot = Value.LastIndexOf('.');
                return dot <= 0 ? Value : Value.Substring(0, dot);
            }
        }
    }

    /// <summary>
    /// Identidad de un objeto del mundo. Formato "kind" o "kind:arg".
    /// PC y XR emiten los mismos ids; la presentación los traduce a casos de uso.
    /// </summary>
    public readonly struct InteractableId
    {
        public string Value { get; }
        public InteractableId(string value) { Value = value ?? ""; }
        public override string ToString() => Value;

        public string Kind
        {
            get
            {
                int colon = Value.IndexOf(':');
                return colon < 0 ? Value : Value.Substring(0, colon);
            }
        }

        public string Arg
        {
            get
            {
                int colon = Value.IndexOf(':');
                return colon < 0 ? "" : Value.Substring(colon + 1);
            }
        }

        public static InteractableId Laptop => new InteractableId("laptop");
        public static InteractableId Tickets => new InteractableId("tickets");
        public static InteractableId Board => new InteractableId("board");
        public static InteractableId Notebook => new InteractableId("notebook");
        public static InteractableId Terminal => new InteractableId("terminal");
        public static InteractableId OutOfScope => new InteractableId("scope");
        public static InteractableId Ticket(string missionId) => new InteractableId("ticket:" + missionId);
        public static InteractableId Clue(string group) => new InteractableId("clue:" + group);
        public static InteractableId Tray(int index) => new InteractableId("tray:" + index);
        public static InteractableId Door => new InteractableId("door");
        public static InteractableId Drawer => new InteractableId("drawer");
    }
}
