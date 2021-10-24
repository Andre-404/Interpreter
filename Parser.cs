﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class parser {
		private List<token> tokens;
		private int current = 0;
		//the parsers input, a list of tokens generated by the scanner
		public parser(List<token> _tokens) {
			tokens = _tokens;
		}
		//for clearing the internal stack when a error occurs
		private class parseError : Exception {

		}

		public List<stmt> parse() {
			//creates a new list of statements, then parses every token and returns the (now full) list of statements
			List<stmt> statements = new List<stmt>();
			while(!isAtEnd()) {
				//declaration is the starting point of every statement
				statements.Add(declaration());
			}

			return statements;
		}
		#region Statements
		private stmt statement() {
			//if we match either a keyword or "{", we consume the token and create a new statement based on the token matched
			if(match(TokenType.IF)) return ifStatement();
			if(match(TokenType.WHILE)) return whileStatement();
			if(match(TokenType.PRINT)) return printStatement();
			if(match(TokenType.FOR)) return forStatement();
			if(match(TokenType.FUN)) return funcStatement("function");
			if(match(TokenType.RETURN)) return returnStatement();
			if(match(TokenType.CLASS)) return classStatement();
			if(match(TokenType.LEFT_BRACE)) return new blockStmt(blockStatement());

			//if we don't match any tokens, it means the current token is part of a expression, so we create a expression statement
			return expressionStatement();
		}

		private stmt declaration() {
			try {
				//if we match a keyword "var", we return a declaration statement
				if(match(TokenType.VAR)) return varDeclaration();
				//otherwise we return a normal statement
				return statement();
			}catch(parseError error) {
				//if we catch a error, we synchronize before we move onto the next statement
				synchronize();
				return null;
			}
		}

		private stmt varDeclaration() {
			//we are expecting a name at the beginning of the variable declaration, if we don't find it, we throw a error
			token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

			//if we match a "=" token after the var name, it means that there is expression and we need to create it
			expr initializer = null;
			if(match(TokenType.EQUAL)) {
				initializer = expression();
			}
			//at the end of the declaration we need a ";"
			consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
			return new varStmt(name, initializer);//we create a new variable statement object with the name and the initializer expression
		}

		private stmt printStatement() {
			expr value = expression();//create a expression for the value we want to print
			consume(TokenType.SEMICOLON, "Expect ';' after value.");//we need a ";" token after the expression
			return new printStmt(value);//return a new print statement object
		}

		private stmt expressionStatement() {
			expr expr = expression();//we know thos statement is a expression, so we simply evaluate the current expression 
			consume(TokenType.SEMICOLON, "Expect ';' after expression.");
			return new expressionStmt(expr);//new expression statement with the created expression inside
		}

		private List<stmt> blockStatement() {
			//a block statement contains multiple statements inside of it
			List<stmt> statements = new List<stmt>();

			//we create new statements either until we find "}" or until we reach EOF
			//we don't need to consume the "{" since we've already done that when we figured out we have a block statement
			while(!check(TokenType.RIGHT_BRACE) && !isAtEnd()) {
				statements.Add(declaration());
			}
			//if we reach the EOF and we don't find a "}", throw a error
			consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
			return statements;//returns the list of statements inside the block
		}

		private stmt ifStatement() {
			consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
			expr condition = expression();
			consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

			stmt thenBranch = statement();
			stmt elseBranch = null;
			if(match(TokenType.ELSE)) {
				elseBranch = statement();
			}

			return new ifStmt(condition, thenBranch, elseBranch);
		}

		private stmt whileStatement() {
			consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
			expr condition = expression();
			consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
			stmt body = statement();

			return new whileStmt(condition, body);
		}

		private stmt forStatement() {
			consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

			stmt initializer;
			if(match(TokenType.SEMICOLON)) {
				initializer = null;
			}else if(match(TokenType.VAR)) {
				initializer = varDeclaration();
			} else {
				initializer = expressionStatement();
			}
			expr condition = null;
			if(!check(TokenType.SEMICOLON)) {
				condition = expression();
			}
			consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

			expr increment = null;
			if(!check(TokenType.RIGHT_PAREN)) {
				increment = expression();
			}
			consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

			stmt body = statement();

			if(increment != null) {
				body = new blockStmt(new List<stmt>() {
					body, new expressionStmt(increment)
				});
			}

			if(condition == null) condition = new literalExpr(true);
			body = new whileStmt(condition, body);

			if(initializer != null) {
				body = new blockStmt(new List<stmt>(){initializer, body });
			}

			return body;
		}

		private funcStmt funcStatement(string kind) {
			token name = consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");
			consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " name.");

			List<token> parameters = new List<token>();
			if(!check(TokenType.RIGHT_PAREN)) {
				do {
					if(parameters.Count >= 255) {
						error(peek(), "Can't have more than 255 parameters.");
					}

					parameters.Add(
						consume(TokenType.IDENTIFIER, "Expect parameter name."));
				} while(match(TokenType.COMMA));
			}
			consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

			consume(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
			List<stmt> body = blockStatement();
			return new funcStmt(name, parameters, body);
		}

		private stmt returnStatement() {
			token keyword = previous();
			expr value = null;
			if(!check(TokenType.SEMICOLON)) {
				value = expression();
			}

			consume(TokenType.SEMICOLON, "Expect ';' after return value.");
			return new returnStmt(keyword, value);
		}

		private stmt classStatement() {
			token name = consume(TokenType.IDENTIFIER, "Expect class name.");
			consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

			List<stmt> methods = new List<stmt>();
			while(!check(TokenType.RIGHT_BRACE) && !isAtEnd()) {
				methods.Add(funcStatement("method"));
			}

			consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

			return new classStmt(name, methods);
		}
		#endregion

		#region expressions
		private expr expression() {
			return assignment();//assigment has the lowest priority
		}

		private expr assignment() {
			//this is the next expression in the precedence 
			expr Expr = or();//if we have a assigment expression, this is the name of the variable

			//if this really is a assigment expression, we consume the "=" token
			if(match(TokenType.EQUAL)) {
				token equals = previous();
				expr value = assignment();//this is the expression we want to set our variable to

				if(Expr is varExpr) {
					token name = ((varExpr)Expr).name;//we get the name of the variable
					return new assignmentExpr(name, value);//we return a new expression that contains the name and the expression it is assigned
				}else if(Expr is getExpr) {
					getExpr get = (getExpr)Expr;
					return new setExpr(get.obj, get.name, value);
				}

				error(equals, "Invalid assignment target.");//if we have a "=" but no expression following it, throw a error
			}

			return Expr;//if this isn't a assigment expression, return whatever or() returns
		}

		private expr or() {
			expr Expr = and();

			//this allows stuff like condition1 or condition2 or condition3
			while(match(TokenType.OR)) {
				token op = previous();
				expr right = and();
				Expr = new logicalExpr(Expr, op, right);
			}

			return Expr;
		}

		private expr and() {
			expr Expr = equality();
			while(match(TokenType.AND)) {
				token op = previous();
				expr right = equality();
				Expr = new logicalExpr(Expr, op, right);
			}

			return Expr;
		}

		private expr equality() {
			//this is next in the precedence list
			expr Expr = comparison();//if this is a equality expression, then this is the left part of it

			//if we match either "!=" or "==" then we know that this is a equality expression
			while(match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
				//gets the token 
				token op = previous();
				expr right = comparison();//generates a expression for the right side of the equality
				Expr = new binaryExpr(Expr, op, right);//new binary expr 
			}

			return Expr;//if this isn't a equality, return whatever comparions() returns
		}

		private bool match(params TokenType[] tokens) {
			//tokens are a array of tokens we wish to check against the current token
			for(int i = 0; i < tokens.Length; i++) {
				TokenType type = tokens[i];
				if(check(type)) {
					//if the current token is any of the provided tokens, then we consume it and return true
					advance();
					return true;
				}
			}
			//if no tokens match the current token, we don't consume it and return false
			return false;
		}

		private bool check(TokenType type) {
			if(isAtEnd()) return false;
			return peek().type == type;//takes a look at the token we have yet to consume and compares it's type to the provided type
		}

		private token advance() {
			//if we're not at EOF, update our position in the token list
			if(!isAtEnd()) current++;
			return previous();
		}

		private bool isAtEnd() {
			return peek().type == TokenType.EOF;
		}

		private token peek() {
			return tokens[current];//returns the token we have yet to consume
		}

		private token previous() {
			return tokens[current - 1];//return the most recently consumed token
		}

		private expr comparison() {
			expr Expr = term();//next in the precedence lever

			//works on the same principle as the equality expression
			while(match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
				token op = previous();
				expr right = term();
				Expr = new binaryExpr(Expr, op, right);
			}

			return Expr;//if this isn't a comparison, return whatver term() returned
		}


		private expr term() {
			expr Expr = factor();//next in precedence 

			//works on the same principle as comparison
			while(match(TokenType.MINUS, TokenType.PLUS)) {
				token op = previous();
				expr right = factor();
				Expr = new binaryExpr(Expr, op, right);
			}

			return Expr;//if this isn't a term(addition/subtraction), return whatever factor() returns
		}

		private expr factor() {
			expr Expr = unary();//next in precedence

			//same as term
			while(match(TokenType.SLASH, TokenType.STAR)) {
				token op = previous();
				expr right = unary();
				Expr = new binaryExpr(Expr, op, right);
			}

			return Expr;//if this isn't factor(mult/div), then return whatever unary() returns
		}

		private expr unary() {
			//since unary consists of -/! and a expression, we first check to see if we have a "!" or "-" ahead
			if(match(TokenType.BANG, TokenType.MINUS)) {
				token op = previous();
				expr right = unary();//here is a little trick we do to allow nested unary expression(--4) by recursivly calling the unary func
				return new unaryExpr(op, right);
			}
			return call();//if this isn't a unary expression, return whatever call() returns
		}

		private expr call() {
			expr Expr = primary();//maximum precedence

			while(true) {
				if(match(TokenType.LEFT_PAREN)) {
					Expr = finishCall(Expr);
				} else if(match(TokenType.DOT)) {
					token name = consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
					Expr = new getExpr(Expr, name);
				} else {
					break;
				}
			}

			return Expr;

		}

		private expr finishCall(expr callee) {
			List<expr> arguments = new List<expr>();
			if(!check(TokenType.RIGHT_PAREN)) {
				do {
					if(arguments.Count >= 255) {
						error(peek(), "Can't have more than 255 arguments.");
					}
					arguments.Add(expression());
				} while(match(TokenType.COMMA));
			}

			token paren = consume(TokenType.RIGHT_PAREN,
								  "Expect ')' after arguments.");

			return new callExpr(callee, paren, arguments);
		}

		private expr primary() {
			//this is the highest precedence 
			//return the literal for numbers, strings and keywords
			if(match(TokenType.FALSE)) return new literalExpr(false);
			if(match(TokenType.TRUE)) return new literalExpr(true);
			if(match(TokenType.NIL)) return new literalExpr(null);
			if(match(TokenType.NUMBER, TokenType.STRING)) {
				return new literalExpr(previous().literal);
			}
			//if this is a identifier, we know that this is a variable name, so we return a expression containing the name of the variable
			if(match(TokenType.IDENTIFIER)) return new varExpr(previous());
			if(match(TokenType.THIS)) return new thisExpr(previous());
			
			//if we have "(" ahead, then we know we have a grouping
			if(match(TokenType.LEFT_PAREN)) {
				expr Expr = expression();//generate a expression inside the parentheses
				consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");//we need to have a ")" after the expression
				
				return new groupingExpr(Expr);//return a new grouping expression
			}

			throw error(peek(), "Expected expression.");
		}

		private token consume(TokenType type, string msg) {
			//if the token ahead is of the type we need, consume it
			if(check(type)) return advance();

			//otherwise, throw a error
			throw error(peek(), msg);
		}

		private parseError error(token t, string msg) {
			Lox.error(t, msg);//this makes sure we don't execute faulty code
			return new parseError();
		}

		private void synchronize() {
			advance();//consume the erroneous token

			//we skip until we encounter either a ";" or some keyword, since then we know we are at a start of a new statement
			while(!isAtEnd()) {
				if(previous().type == TokenType.SEMICOLON)
					return;

				switch(peek().type) {
					case TokenType.CLASS:
					case TokenType.FUN:
					case TokenType.VAR:
					case TokenType.FOR:
					case TokenType.IF:
					case TokenType.WHILE:
					case TokenType.PRINT:
					case TokenType.RETURN:
						return;
				}

				advance();
			}
		}

		#endregion
	}
}
