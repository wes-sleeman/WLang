REF 'IO', 'Util' FROM 'Runtime'

{
	Tests function creation.
	Creates a private and public function, then calls them.
}

MyFunc("Success!")
MyFunc2("one", "two", "three")

Public Func MyFunc
[
	Type("Output public: " args)
]

Func MyFunc2
[
	Type("Output private:\n\t" Thread("\n\t" args))
]
