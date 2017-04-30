{
	This is a simple W program to demonstrate some of the basic functionality.
	Expected output:
	Welcome to the W Programming Language
	What is your name?
	(Waits for input)
	(Pause a beat, Screen clears)
	Hello, <name>!
	(Pause, then terminate program)
}

type "Welcome to the "
forecolor = "red"
backcolor = "blue"
type "W Programming Language"
resetcolors
newline
pause 1
type "What is your name?"
newline
read
pause 1
reset
type "Hello, "
type $
type "!"
pause 3 + 2
reset
