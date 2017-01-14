#Comment header
head_one: one_lib two three \
	four five

head_one_path=head/one
head_one_prefix=Tsd.head.one

#Comment two
two: three
	dodo

# Note the space, will be removed 
four : three 
	dodo

five: three two four one_lib
	dodo


three: one_lib
	Do Number three
	#Some this then space

	#Last line of three



