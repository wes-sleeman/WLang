REF 'IO' FROM 'Runtime'

{
	Standard test algorithm
	Prints Fibonacci numbers.
}

? Declare x & y = 1
Item x = 1
Item y = 1

? Type 1 (the first fib num)
Type(x)

? Generate the next 15 numbers
Repeat 15
[
	? Type the next number
	Type(y)
	
	? Z = x + y
	Item z = x + y
	
	? Move the variables down
	x = y
	y = z
]