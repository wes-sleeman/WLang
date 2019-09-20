REF 'IO', 'Util' FROM 'Runtime'

Item List = Collect('s', '!')
List = Collect('s', List)
List = Collect('e', List)
List = Collect('c', List)
List = Collect('c', List)
List = Collect('u', List)
List = Collect('S', List)

Type(List)