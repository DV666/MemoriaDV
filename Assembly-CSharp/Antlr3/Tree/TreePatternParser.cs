﻿/*
 * [The "BSD licence"]
 * Copyright (c) 2005-2008 Terence Parr
 * All rights reserved.
 *
 * Conversion to C#:
 * Copyright (c) 2008-2009 Sam Harwell, Pixel Mine, Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace Antlr.Runtime.Tree
{
    using InvalidOperationException = System.InvalidOperationException;

    public class TreePatternParser
    {
        protected TreePatternLexer tokenizer;
        protected int ttype;
        protected TreeWizard wizard;
        protected ITreeAdaptor adaptor;

        public TreePatternParser(TreePatternLexer tokenizer, TreeWizard wizard, ITreeAdaptor adaptor)
        {
            this.tokenizer = tokenizer;
            this.wizard = wizard;
            this.adaptor = adaptor;
            ttype = tokenizer.NextToken(); // kickstart
        }

        public virtual object Pattern()
        {
            if (ttype == TreePatternLexer.Begin)
            {
                return ParseTree();
            }
            else if (ttype == TreePatternLexer.Id)
            {
                object node = ParseNode();
                if (ttype == CharStreamConstants.EndOfFile)
                {
                    return node;
                }
                return null; // extra junk on end
            }
            return null;
        }

        public virtual object ParseTree()
        {
            if (ttype != TreePatternLexer.Begin)
                throw new InvalidOperationException("No beginning.");

            ttype = tokenizer.NextToken();
            object root = ParseNode();
            if (root == null)
            {
                return null;
            }
            while (ttype == TreePatternLexer.Begin ||
                    ttype == TreePatternLexer.Id ||
                    ttype == TreePatternLexer.Percent ||
                    ttype == TreePatternLexer.Dot)
            {
                if (ttype == TreePatternLexer.Begin)
                {
                    object subtree = ParseTree();
                    adaptor.AddChild(root, subtree);
                }
                else
                {
                    object child = ParseNode();
                    if (child == null)
                    {
                        return null;
                    }
                    adaptor.AddChild(root, child);
                }
            }

            if (ttype != TreePatternLexer.End)
                throw new InvalidOperationException("No end.");

            ttype = tokenizer.NextToken();
            return root;
        }

        public virtual object ParseNode()
        {
            // "%label:" prefix
            string label = null;
            if (ttype == TreePatternLexer.Percent)
            {
                ttype = tokenizer.NextToken();
                if (ttype != TreePatternLexer.Id)
                {
                    return null;
                }
                label = tokenizer.sval.ToString();
                ttype = tokenizer.NextToken();
                if (ttype != TreePatternLexer.Colon)
                {
                    return null;
                }
                ttype = tokenizer.NextToken(); // move to ID following colon
            }

            // Wildcard?
            if (ttype == TreePatternLexer.Dot)
            {
                ttype = tokenizer.NextToken();
                IToken wildcardPayload = new CommonToken(0, ".");
                TreeWizard.TreePattern node =
                    new TreeWizard.WildcardTreePattern(wildcardPayload);
                if (label != null)
                {
                    node.label = label;
                }
                return node;
            }

            // "ID" or "ID[arg]"
            if (ttype != TreePatternLexer.Id)
            {
                return null;
            }
            string tokenName = tokenizer.sval.ToString();
            ttype = tokenizer.NextToken();
            if (tokenName.Equals("nil"))
            {
                return adaptor.Nil();
            }
            string text = tokenName;
            // check for arg
            string arg = null;
            if (ttype == TreePatternLexer.Arg)
            {
                arg = tokenizer.sval.ToString();
                text = arg;
                ttype = tokenizer.NextToken();
            }

            // create node
            int treeNodeType = wizard.GetTokenType(tokenName);
            if (treeNodeType == TokenTypes.Invalid)
            {
                return null;
            }
            object node2;
            node2 = adaptor.Create(treeNodeType, text);
            if (label != null && node2.GetType() == typeof(TreeWizard.TreePattern))
            {
                ((TreeWizard.TreePattern)node2).label = label;
            }
            if (arg != null && node2.GetType() == typeof(TreeWizard.TreePattern))
            {
                ((TreeWizard.TreePattern)node2).hasTextArg = true;
            }
            return node2;
        }
    }
}
