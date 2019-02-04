REF 'IO' FROM 'Runtime'

{
	Prints the factorial of the
		first fifteen numbers.
}

Repeat 15
[
	? Start x at 1
	Item x = 1
	
	? Repeat # (loop counter) times
	Repeat #
	[
		? Multiply x by # (the inside loop counter)
		x = x * (# + 1)
	]
	? Print x to the console.
	Type(x)
]