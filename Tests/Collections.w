REF 'IO', 'Util' FROM 'Runtime'

Item arrofarrs = Collect(Collect(1,2,3),Collect(4,5,6),Collect(7,8,9))

Repeat arrofarrs.Num
[
	Item x = #
	Repeat arrofarrs.(#).Num
	[
		Type(arrofarrs.(x).(#))
	]
	Type("-------------------")
]

Type("Success!")