REF 'IO' FROM 'Runtime'

{
	Prints out all args.
	If no args are specified, prints "No arguments found!"
}

? Check number of arguments
If Args.Num > 0
[
	? For the number of arguments
	Repeat Args.Num
	[
		? Print the argument at # (iteration counter)
		Type(Args.(#))
	]
]
? Else
[
	? Print a failure message
	Type("No arguments found!")
]