﻿namespace Lanceur.Infra.SQLite
{
    internal record Tuple<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
    }
}