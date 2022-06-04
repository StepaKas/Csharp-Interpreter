grammar Grammar;

program: line* EOF;

line: statement | ifBlock | whileBlock;

statement: (declare | assignment  | functionCall  )? ';';

ifBlock: 'if'  '(' expression ')' (line |   block )    ('else'  elseIfBlock )?;

elseIfBlock: line |  block  | ifBlock ;

whileBlock: 'while' '(' expression ')' (  block   | line )   ;

declare: datatype IDENTIFIER ((',' IDENTIFIER)*)? ('=' expression)?;

assignment:  IDENTIFIER (('='IDENTIFIER)*)?'=' expression;

functionCall: IDENTIFIER  (expression (',' expression)*)? ;

expression
	:  constant								#constantExpression
	| IDENTIFIER							#identifierExpression
	| functionCall							#functionCallExpression
	| '(' expression ')'					#parenthesizedExpression
	| '!' expression						#notExpression
	| '-' expression						#unaryMinusExpression
	| expression multOp expression			#multiplicativeExpression
	| expression addOp expression			#additiveExpression
	| expression compareOp expression		#comparisonExpression
	| expression boolOp expression			#booleanExpression
	;

fragment
COMMENT
: '/*'.*'*/' /*single comment*/
| '//'~('\r' | '\n')* /* multiple comment*/
;

multOp: '*' | '/' | '%' ;
addOp: '+' | '-' | '.' ;
compareOp: '==' | '!=' | '>' | '<' | '>=' | '<=' ;
boolOp: BOOL_OPERATOR;

BOOL_OPERATOR: '&&' | '||' ;

datatype: 'int' | 'string' | 'float' | 'bool';
constant: INTEGER | FLOAT | STRING | BOOL | NULL;

INTEGER: DECIMAL | HEXA | OCT;
FLOAT: [0-9]+ '.' [0-9]+;
STRING: ('"' ~'"'* '"') | ( '\'' ~'\''* '\'');
BOOL: 'true' | 'false';
NULL: 'null';

DECIMAL: '-'?[0-9]+;
HEXA: '0x'[A-Fa-f0-9]+;
OCT: '0'[0-7]*;

block: '{' line* '}';

WS : ([ \r\n\t]+ | COMMENT) -> skip ;   // toss out whitespace
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;

