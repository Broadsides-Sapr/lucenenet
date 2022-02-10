﻿using Lucene.Net.QueryParsers.Surround.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#if FEATURE_SERIALIZABLE_EXCEPTIONS
using System.Runtime.Serialization;
#endif
using JCG = J2N.Collections.Generic;

namespace Lucene.Net.QueryParsers.Surround.Parser
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    /// <summary>
    /// This class is generated by JavaCC.  The only method that clients should need
    /// to call is <see cref="Parse(string)"/>.
    ///
    /// <para>
    /// This parser generates queries that make use of position information
    /// (Span queries). It provides positional operators (<c>w</c> and
    /// <c>n</c>) that accept a numeric distance, as well as boolean
    /// operators (<c>and</c>, <c>or</c>, and <c>not</c>,
    /// wildcards (<c>///</c> and <c>?</c>), quoting (with
    /// <c>"</c>), and boosting (via <c>^</c>).
    /// </para>
    ///
    /// <para>
    /// The operators (W, N, AND, OR, NOT) can be expressed lower-cased or
    /// upper-cased, and the non-unary operators (everything but NOT) support
    /// both infix <c>(a AND b AND c)</c> and prefix <c>AND(a, b,
    /// c)</c> notation.
    /// </para>
    ///
    /// <para>
    /// The W and N operators express a positional relationship among their
    /// operands.  N is ordered, and W is unordered.  The distance is 1 by
    /// default, meaning the operands are adjacent, or may be provided as a
    /// prefix from 2-99.  So, for example, 3W(a, b) means that terms a and b
    /// must appear within three positions of each other, or in other words, up
    /// to two terms may appear between a and b. 
    /// </para>
    /// </summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This class is based on generated code")]
    [SuppressMessage("Style", "IDE0028:Collection initialization can be simplified", Justification = "This class is based on generated code")]
    public class QueryParser
    {
        internal readonly int minimumPrefixLength = 3;
        internal readonly int minimumCharsInTrunc = 3;
        internal readonly string truncationErrorMessage = "Too unrestrictive truncation: ";
        internal readonly string boostErrorMessage = "Cannot handle boost value: ";

        /* CHECKME: These should be the same as for the tokenizer. How? */
        internal readonly char truncator = '*';
        internal readonly char anyChar = '?';
        internal readonly char quote = '"';
        internal readonly char fieldOperator = ':';
        internal readonly char comma = ','; /* prefix list separator */
        internal readonly char carat = '^'; /* weight operator */

        public static SrndQuery Parse(string query)
        {
            QueryParser parser = new QueryParser();
            return parser.Parse2(query);
        }

        public QueryParser()
            : this(new FastCharStream(new StringReader("")))
        {
        }

        public virtual SrndQuery Parse2(string query)
        {
            ReInit(new FastCharStream(new StringReader(query)));
            try
            {
                return TopSrndQuery();
            }
            catch (TokenMgrError tme)
            {
                throw new ParseException(tme.Message);
            }
        }

        protected virtual SrndQuery GetFieldsQuery(
            SrndQuery q, IList<string> fieldNames)
        {
            /* FIXME: check acceptable subquery: at least one subquery should not be
             * a fields query.
             */
            return new FieldsQuery(q, fieldNames, fieldOperator);
        }

        protected virtual SrndQuery GetOrQuery(IList<SrndQuery> queries, bool infix, Token orToken)
        {
            return new OrQuery(queries, infix, orToken.Image);
        }

        protected virtual SrndQuery GetAndQuery(IList<SrndQuery> queries, bool infix, Token andToken)
        {
            return new AndQuery(queries, infix, andToken.Image);
        }

        protected virtual SrndQuery GetNotQuery(IList<SrndQuery> queries, Token notToken)
        {
            return new NotQuery(queries, notToken.Image);
        }

        protected static int GetOpDistance(string distanceOp)
        {
            /* W, 2W, 3W etc -> 1, 2 3, etc. Same for N, 2N ... */
            return distanceOp.Length == 1
              ? 1
              : int.Parse(distanceOp.Substring(0, distanceOp.Length - 1)); // LUCENENET TODO: Culture from current thread?
        }

        protected static void CheckDistanceSubQueries(DistanceQuery distq, string opName)
        {
            string m = distq.DistanceSubQueryNotAllowed();
            if (m != null)
            {
                throw new ParseException("Operator " + opName + ": " + m);
            }
        }

        protected virtual SrndQuery GetDistanceQuery(
            IList<SrndQuery> queries,
            bool infix,
            Token dToken,
            bool ordered)
        {
            DistanceQuery dq = new DistanceQuery(queries,
                                                infix,
                                                GetOpDistance(dToken.Image),
                                                dToken.Image,
                                                ordered);
            CheckDistanceSubQueries(dq, dToken.Image);
            return dq;
        }

        protected virtual SrndQuery GetTermQuery(
              string term, bool quoted)
        {
            return new SrndTermQuery(term, quoted);
        }

        protected virtual bool AllowedSuffix(string suffixed)
        {
            return (suffixed.Length - 1) >= minimumPrefixLength;
        }

        protected virtual SrndQuery GetPrefixQuery(
            string prefix, bool quoted)
        {
            return new SrndPrefixQuery(prefix, quoted, truncator);
        }

        protected virtual bool AllowedTruncation(string truncated)
        {
            /* At least 3 normal characters needed. */
            int nrNormalChars = 0;
            for (int i = 0; i < truncated.Length; i++)
            {
                char c = truncated[i];
                if ((c != truncator) && (c != anyChar))
                {
                    nrNormalChars++;
                }
            }
            return nrNormalChars >= minimumCharsInTrunc;
        }

        protected virtual SrndQuery GetTruncQuery(string truncated)
        {
            return new SrndTruncQuery(truncated, truncator, anyChar);
        }

        public SrndQuery TopSrndQuery()
        {
            SrndQuery q;
            q = FieldsQuery();
            Jj_consume_token(0);
            { if (true) return q; }
            throw Error.Create("Missing return statement in function");
        }

        public SrndQuery FieldsQuery()
        {
            SrndQuery q;
            IList<string> fieldNames;
            fieldNames = OptionalFields();
            q = OrQuery();
            { if (true) return (fieldNames is null) ? q : GetFieldsQuery(q, fieldNames); }
            throw Error.Create("Missing return statement in function");
        }

        public IList<string> OptionalFields()
        {
            Token fieldName;
            IList<string> fieldNames = null;

            while (true)
            {
                if (Jj_2_1(2))
                {
                    ;
                }
                else
                {
                    goto label_1;
                }
                // to the colon
                fieldName = Jj_consume_token(RegexpToken.TERM);
                Jj_consume_token(RegexpToken.COLON);
                if (fieldNames is null)
                {
                    fieldNames = new JCG.List<string>();
                }
                fieldNames.Add(fieldName.Image);
            }
        label_1:
            { if (true) return fieldNames; }
            throw Error.Create("Missing return statement in function");
        }

        public SrndQuery OrQuery()
        {
            SrndQuery q;
            IList<SrndQuery> queries = null;
            Token oprt = null;
            q = AndQuery();

            while (true)
            {
                switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
                {
                    case RegexpToken.OR:
                        ;
                        break;
                    default:
                        jj_la1[0] = jj_gen;
                        goto label_2;
                }
                oprt = Jj_consume_token(RegexpToken.OR);
                /* keep only last used operator */
                if (queries is null)
                {
                    queries = new JCG.List<SrndQuery>();
                    queries.Add(q);
                }
                q = AndQuery();
                queries.Add(q);
            }
        label_2:
            { if (true) return (queries is null) ? q : GetOrQuery(queries, true /* infix */, oprt); }
            throw Error.Create("Missing return statement in function");
        }

        public SrndQuery AndQuery()
        {
            SrndQuery q;
            IList<SrndQuery> queries = null;
            Token oprt = null;
            q = NotQuery();

            while (true)
            {
                switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
                {
                    case RegexpToken.AND:
                        ;
                        break;
                    default:
                        jj_la1[1] = jj_gen;
                        goto label_3;
                }
                oprt = Jj_consume_token(RegexpToken.AND);
                /* keep only last used operator */
                if (queries is null)
                {
                    queries = new JCG.List<SrndQuery>();
                    queries.Add(q);
                }
                q = NotQuery();
                queries.Add(q);
            }
        label_3:
            { if (true) return (queries is null) ? q : GetAndQuery(queries, true /* infix */, oprt); }
            throw Error.Create("Missing return statement in function");
        }

        public SrndQuery NotQuery()
        {
            SrndQuery q;
            IList<SrndQuery> queries = null;
            Token oprt = null;
            q = NQuery();

            while (true)
            {
                switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
                {
                    case RegexpToken.NOT:
                        ;
                        break;
                    default:
                        jj_la1[2] = jj_gen;
                        goto label_4;
                }
                oprt = Jj_consume_token(RegexpToken.NOT);
                /* keep only last used operator */
                if (queries is null)
                {
                    queries = new JCG.List<SrndQuery>();
                    queries.Add(q);
                }
                q = NQuery();
                queries.Add(q);
            }
        label_4:
            { if (true) return (queries is null) ? q : GetNotQuery(queries, oprt); }
            throw Error.Create("Missing return statement in function");
        }

        public SrndQuery NQuery()
        {
            SrndQuery q;
            IList<SrndQuery> queries;
            Token dt;
            q = WQuery();

            while (true)
            {
                switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
                {
                    case RegexpToken.N:
                        ;
                        break;
                    default:
                        jj_la1[3] = jj_gen;
                        goto label_5;
                }
                dt = Jj_consume_token(RegexpToken.N);
                queries = new JCG.List<SrndQuery>();
                queries.Add(q); /* left associative */

                q = WQuery();
                queries.Add(q);
                q = GetDistanceQuery(queries, true /* infix */, dt, false /* not ordered */);
            }
        label_5:
            { if (true) return q; }
            throw Error.Create("Missing return statement in function");
        }

        public SrndQuery WQuery()
        {
            SrndQuery q;
            IList<SrndQuery> queries;
            Token wt;
            q = PrimaryQuery();

            while (true)
            {
                switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
                {
                    case RegexpToken.W:
                        ;
                        break;
                    default:
                        jj_la1[4] = jj_gen;
                        goto label_6;
                }
                wt = Jj_consume_token(RegexpToken.W);
                queries = new JCG.List<SrndQuery>();
                queries.Add(q); /* left associative */

                q = PrimaryQuery();
                queries.Add(q);
                q = GetDistanceQuery(queries, true /* infix */, wt, true /* ordered */);
            }
        label_6:
            { if (true) return q; }
            throw Error.Create("Missing return statement in function");
        }

        public SrndQuery PrimaryQuery()
        {
            /* bracketed weighted query or weighted term */
            SrndQuery q;
            switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
            {
                case RegexpToken.LPAREN:
                    Jj_consume_token(RegexpToken.LPAREN);
                    q = FieldsQuery();
                    Jj_consume_token(RegexpToken.RPAREN);
                    break;
                case RegexpToken.OR:
                case RegexpToken.AND:
                case RegexpToken.W:
                case RegexpToken.N:
                    q = PrefixOperatorQuery();
                    break;
                case RegexpToken.TRUNCQUOTED:
                case RegexpToken.QUOTED:
                case RegexpToken.SUFFIXTERM:
                case RegexpToken.TRUNCTERM:
                case RegexpToken.TERM:
                    q = SimpleTerm();
                    break;
                default:
                    jj_la1[5] = jj_gen;
                    Jj_consume_token(-1);
                    throw new ParseException();
            }
            OptionalWeights(q);
            { if (true) return q; }
            throw Error.Create("Missing return statement in function");
        }

        public SrndQuery PrefixOperatorQuery()
        {
            Token oprt;
            IList<SrndQuery> queries;
            switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
            {
                case RegexpToken.OR:
                    oprt = Jj_consume_token(RegexpToken.OR);
                    /* prefix OR */
                    queries = FieldsQueryList();
                    { if (true) return GetOrQuery(queries, false /* not infix */, oprt); }
                    //break; // unreachable
                case RegexpToken.AND:
                    oprt = Jj_consume_token(RegexpToken.AND);
                    /* prefix AND */
                    queries = FieldsQueryList();
                    { if (true) return GetAndQuery(queries, false /* not infix */, oprt); }
                    //break; // unreachable
                case RegexpToken.N:
                    oprt = Jj_consume_token(RegexpToken.N);
                    /* prefix N */
                    queries = FieldsQueryList();
                    { if (true) return GetDistanceQuery(queries, false /* not infix */, oprt, false /* not ordered */); }
                    //break; // unreachable
                case RegexpToken.W:
                    oprt = Jj_consume_token(RegexpToken.W);
                    /* prefix W */
                    queries = FieldsQueryList();
                    { if (true) return GetDistanceQuery(queries, false  /* not infix */, oprt, true /* ordered */); }
                    //break; // unreachable
                default:
                    jj_la1[6] = jj_gen;
                    Jj_consume_token(-1);
                    throw new ParseException();
            }
            throw Error.Create("Missing return statement in function");
        }

        public IList<SrndQuery> FieldsQueryList()
        {
            SrndQuery q;
            IList<SrndQuery> queries = new JCG.List<SrndQuery>();
            Jj_consume_token(RegexpToken.LPAREN);
            q = FieldsQuery();
            queries.Add(q);

            while (true)
            {
                Jj_consume_token(RegexpToken.COMMA);
                q = FieldsQuery();
                queries.Add(q);
                switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
                {
                    case RegexpToken.COMMA:
                        ;
                        break;
                    default:
                        jj_la1[7] = jj_gen;
                        goto label_7;
                }
            }
        label_7:
            Jj_consume_token(RegexpToken.RPAREN);
            { if (true) return queries; }
            throw Error.Create("Missing return statement in function");
        }

        public SrndQuery SimpleTerm()
        {
            Token term;
            switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
            {
                case RegexpToken.TERM:
                    term = Jj_consume_token(RegexpToken.TERM);
                    { if (true) return GetTermQuery(term.Image, false /* not quoted */); }
                    //break; // unreachable
                case RegexpToken.QUOTED:
                    term = Jj_consume_token(RegexpToken.QUOTED);
                    { if (true) return GetTermQuery(term.Image.Substring(1, (term.Image.Length - 1) - 1), true /* quoted */); }
                    //break; // unreachable
                case RegexpToken.SUFFIXTERM:
                    term = Jj_consume_token(RegexpToken.SUFFIXTERM);
                    /* ending in * */
                    if (!AllowedSuffix(term.Image))
                    {
                        { if (true) throw new ParseException(truncationErrorMessage + term.Image); }
                    }
                    { if (true) return GetPrefixQuery(term.Image.Substring(0, term.Image.Length - 1), false /* not quoted */); }
                    //break; // unreachable
                case RegexpToken.TRUNCTERM:
                    term = Jj_consume_token(RegexpToken.TRUNCTERM);
                    /* with at least one * or ? */
                    if (!AllowedTruncation(term.Image))
                    {
                        { if (true) throw new ParseException(truncationErrorMessage + term.Image); }
                    }
                    { if (true) return GetTruncQuery(term.Image); }
                    //break; // unreachable
                case RegexpToken.TRUNCQUOTED:
                    term = Jj_consume_token(RegexpToken.TRUNCQUOTED);
                    /* eg. "9b-b,m"* */
                    if ((term.Image.Length - 3) < minimumPrefixLength)
                    {
                        { if (true) throw new ParseException(truncationErrorMessage + term.Image); }
                    }
                    { if (true) return GetPrefixQuery(term.Image.Substring(1, (term.Image.Length - 2) - 1), true /* quoted */); }
                    //break; // unreachable
                default:
                    jj_la1[8] = jj_gen;
                    Jj_consume_token(-1);
                    throw new ParseException();
            }
            throw Error.Create("Missing return statement in function");
        }

        public void OptionalWeights(SrndQuery q)
        {
            Token weight; // LUCENENET: IDE0059: Remove unnecessary value assignment
            while (true)
            {
                switch ((jj_ntk == -1) ? Jj_ntk() : jj_ntk)
                {
                    case RegexpToken.CARAT:
                        ;
                        break;
                    default:
                        jj_la1[9] = jj_gen;
                        goto label_8;
                }
                Jj_consume_token(RegexpToken.CARAT);
                weight = Jj_consume_token(RegexpToken.NUMBER);
                float f;
                try
                {
                    // LUCENENET TODO: Test parsing float in various cultures (.NET)
                    f = float.Parse(weight.Image);
                }
                catch (Exception floatExc) // LUCENENET: No need to call the IsException() extension method here because we are dealing only with a .NET platform method
                {
                    { if (true) throw new ParseException(boostErrorMessage + weight.Image + " (" + floatExc + ")", floatExc); }
                }
                if (f <= 0.0)
                {
                    { if (true) throw new ParseException(boostErrorMessage + weight.Image); }
                }
                q.Weight = (f * q.Weight); /* left associative, fwiw */
            }
        label_8: ;
        }

        private bool Jj_2_1(int xla)
        {
            jj_la = xla; jj_lastpos = jj_scanpos = Token;
            try { return !Jj_3_1(); }
            catch (LookaheadSuccess) { return true; }
            finally { Jj_save(0, xla); }
        }

        private bool Jj_3_1()
        {
            if (Jj_scan_token(RegexpToken.TERM)) return true;
            if (Jj_scan_token(RegexpToken.COLON)) return true;
            return false;
        }

        /// <summary>Generated Token Manager.</summary>
        public QueryParserTokenManager TokenSource { get; set; }
        /// <summary>Current token.</summary>
        public Token Token { get; set; }
        /// <summary>Next token.</summary>
        public Token Jj_nt { get; set; }
        private int jj_ntk;
        private Token jj_scanpos, jj_lastpos;
        private int jj_la;
        private int jj_gen;
        private readonly int[] jj_la1 = new int[10];
        private static readonly int[] jj_la1_0 = new int[] { // LUCENENET: marked readonly // LUCENENET: Avoid static constructors (see https://github.com/apache/lucenenet/pull/224#issuecomment-469284006)
            0x100, 0x200, 0x400, 0x1000, 0x800, 0x7c3b00, 0x1b00, 0x8000, 0x7c0000, 0x20000, };

        // LUCENENET: Avoid static constructors (see https://github.com/apache/lucenenet/pull/224#issuecomment-469284006)
        //static QueryParser()
        //{
        //    Jj_la1_init_0();
        //}

        //private static void Jj_la1_init_0()
        //{
        //    jj_la1_0 = new int[] { 0x100, 0x200, 0x400, 0x1000, 0x800, 0x7c3b00, 0x1b00, 0x8000, 0x7c0000, 0x20000, };
        //}
        private readonly JJCalls[] jj_2_rtns = new JJCalls[1];
        private bool jj_rescan = false;
        private int jj_gc = 0;

        /// <summary>Constructor with user supplied <see cref="ICharStream"/>.</summary>
        public QueryParser(ICharStream stream)
        {
            TokenSource = new QueryParserTokenManager(stream);
            Token = new Token();
            jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 10; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        /// <summary>Reinitialize.</summary>
        public virtual void ReInit(ICharStream stream)
        {
            TokenSource.ReInit(stream);
            Token = new Token();
            jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 10; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        /// <summary>Constructor with generated Token Manager.</summary>
        public QueryParser(QueryParserTokenManager tm)
        {
            TokenSource = tm;
            Token = new Token();
            jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 10; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        /// <summary>Reinitialize.</summary>
        public virtual void ReInit(QueryParserTokenManager tm)
        {
            TokenSource = tm;
            Token = new Token();
            jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 10; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        private Token Jj_consume_token(int kind)
        {
            Token oldToken;
            if ((oldToken = Token).Next != null) Token = Token.Next;
            else Token = Token.Next = TokenSource.GetNextToken();
            jj_ntk = -1;
            if (Token.Kind == kind)
            {
                jj_gen++;
                if (++jj_gc > 100)
                {
                    jj_gc = 0;
                    for (int i = 0; i < jj_2_rtns.Length; i++)
                    {
                        JJCalls c = jj_2_rtns[i];
                        while (c != null)
                        {
                            if (c.gen < jj_gen) c.first = null;
                            c = c.next;
                        }
                    }
                }
                return Token;
            }
            Token = oldToken;
            jj_kind = kind;
            throw GenerateParseException();
        }

        // LUCENENET: It is no longer good practice to use binary serialization. 
        // See: https://github.com/dotnet/corefx/issues/23584#issuecomment-325724568
#if FEATURE_SERIALIZABLE_EXCEPTIONS
        [Serializable]
#endif
        private sealed class LookaheadSuccess : Exception
        {
            public LookaheadSuccess()
            { }

#if FEATURE_SERIALIZABLE_EXCEPTIONS
            /// <summary>
            /// Initializes a new instance of this class with serialized data.
            /// </summary>
            /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
            /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
            public LookaheadSuccess(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
#endif
        }


        private readonly LookaheadSuccess jj_ls = new LookaheadSuccess();

        private bool Jj_scan_token(int kind)
        {
            if (jj_scanpos == jj_lastpos)
            {
                jj_la--;
                if (jj_scanpos.Next is null)
                {
                    jj_lastpos = jj_scanpos = jj_scanpos.Next = TokenSource.GetNextToken();
                }
                else
                {
                    jj_lastpos = jj_scanpos = jj_scanpos.Next;
                }
            }
            else
            {
                jj_scanpos = jj_scanpos.Next;
            }
            if (jj_rescan)
            {
                int i = 0; Token tok = Token;
                while (tok != null && tok != jj_scanpos) { i++; tok = tok.Next; }
                if (tok != null) Jj_add_error_token(kind, i);
            }
            if (jj_scanpos.Kind != kind) return true;
            if (jj_la == 0 && jj_scanpos == jj_lastpos) throw jj_ls;
            return false;
        }

        /// <summary>Get the next Token.</summary>
        public Token GetNextToken()
        {
            if (Token.Next != null) Token = Token.Next;
            else Token = Token.Next = TokenSource.GetNextToken();
            jj_ntk = -1;
            jj_gen++;
            return Token;
        }

        /// <summary>Get the specific Token.</summary>
        public Token GetToken(int index)
        {
            Token t = Token;
            for (int i = 0; i < index; i++)
            {
                if (t.Next != null) t = t.Next;
                else t = t.Next = TokenSource.GetNextToken();
            }
            return t;
        }

        private int Jj_ntk()
        {
            if ((Jj_nt = Token.Next) is null)
                return (jj_ntk = (Token.Next = TokenSource.GetNextToken()).Kind);
            else
                return (jj_ntk = Jj_nt.Kind);
        }

        private readonly IList<int[]> jj_expentries = new JCG.List<int[]>(); // LUCENENET: marked readonly
        private int[] jj_expentry;
        private int jj_kind = -1;
        private readonly int[] jj_lasttokens = new int[100]; // LUCENENET: marked readonly
        private int jj_endpos;

        private void Jj_add_error_token(int kind, int pos)
        {
            if (pos >= 100) return;
            if (pos == jj_endpos + 1)
            {
                jj_lasttokens[jj_endpos++] = kind;
            }
            else if (jj_endpos != 0)
            {
                jj_expentry = new int[jj_endpos];
                for (int i = 0; i < jj_endpos; i++)
                {
                    jj_expentry[i] = jj_lasttokens[i];
                }
                foreach (var oldentry in jj_expentries)
                {
                    if (oldentry.Length == jj_expentry.Length)
                    {
                        for (int i = 0; i < jj_expentry.Length; i++)
                        {
                            if (oldentry[i] != jj_expentry[i])
                            {
                                goto jj_entries_loop_continue;
                            }
                        }
                        jj_expentries.Add(jj_expentry);
                        goto jj_entries_loop_break;
                    }
                jj_entries_loop_continue: ;
                }
            jj_entries_loop_break:
                if (pos != 0) jj_lasttokens[(jj_endpos = pos) - 1] = kind;
            }
        }

        /// <summary>Generate ParseException.</summary>
        public virtual ParseException GenerateParseException()
        {
            jj_expentries.Clear();
            bool[] la1tokens = new bool[24];
            if (jj_kind >= 0)
            {
                la1tokens[jj_kind] = true;
                jj_kind = -1;
            }
            for (int i = 0; i < 10; i++)
            {
                if (jj_la1[i] == jj_gen)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        if ((jj_la1_0[i] & (1 << j)) != 0)
                        {
                            la1tokens[j] = true;
                        }
                    }
                }
            }
            for (int i = 0; i < 24; i++)
            {
                if (la1tokens[i])
                {
                    jj_expentry = new int[1];
                    jj_expentry[0] = i;
                    jj_expentries.Add(jj_expentry);
                }
            }
            jj_endpos = 0;
            Jj_rescan_token();
            Jj_add_error_token(0, 0);
            int[][] exptokseq = new int[jj_expentries.Count][];
            for (int i = 0; i < jj_expentries.Count; i++)
            {
                exptokseq[i] = jj_expentries[i];
            }
            return new ParseException(Token, exptokseq, QueryParserConstants.TokenImage);
        }

        /// <summary>Enable tracing. </summary>
        public void Enable_tracing()
        {
        }

        /// <summary>Disable tracing. </summary>
        public void Disable_tracing()
        {
        }

        private void Jj_rescan_token()
        {
            jj_rescan = true;
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    JJCalls p = jj_2_rtns[i];
                    do
                    {
                        if (p.gen > jj_gen)
                        {
                            jj_la = p.arg; jj_lastpos = jj_scanpos = p.first;
                            switch (i)
                            {
                                case 0: Jj_3_1(); break;
                            }
                        }
                        p = p.next;
                    } while (p != null);
                }
                catch (LookaheadSuccess /*ls*/) { }
            }
            jj_rescan = false;
        }

        private void Jj_save(int index, int xla)
        {
            JJCalls p = jj_2_rtns[index];
            while (p.gen > jj_gen)
            {
                if (p.next is null) { p = p.next = new JJCalls(); break; }
                p = p.next;
            }
            p.gen = jj_gen + xla - jj_la; p.first = Token; p.arg = xla;
        }

        internal sealed class JJCalls
        {
            internal int gen;
            internal Token first;
            internal int arg;
            internal JJCalls next;
        }
    }
}
