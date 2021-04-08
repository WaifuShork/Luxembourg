﻿namespace Luxembourg
{
    public enum TokenType
    {
        // Single character tokens
        OpenParen,
        CloseParen,
        OpenBrace,
        CloseBrace,
        Comma,
        Dot,
        Minus,
        Plus,
        Semicolon,
        Slash,
        Star,
        
        // One or two character tokens
        Bang,
        BangEquals,
        Equal,
        EqualEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        
        // Literals
        Identifier, 
        String,
        Number,
        
        // Keywords
        And,
        Class,
        Else,
        False,
        Procedure,
        For,
        If,
        Nil,
        Or,
        Print,
        Return,
        Base,
        This,
        True,
        Var,
        While,
        
        EndOfFile,
    }
}