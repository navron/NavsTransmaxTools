#This is a comment
#This is a comment line 2
core_one: one two

core_one_path=ac/as
core_one_prefix=Tsd.core.one

one: other two
	as/blddll $(core_one_path)/$@ $@
	scripts/cpy $(core_one_path)/$@/release/$@.dll $(AS_TARGET_BIN)
	scripts/cpy $(core_one_path)/$@/release/$@.pdb $(AS_TARGET_BIN)
	# leaving a line space

	# last line of this project

two: other
	as/bldsvc $(core_one_path)/$@ $@
	scripts/cpy $(core_one_path)/$@/release/$@.exe $(AS_TARGET_BIN)
	scripts/cpy $(core_one_path)/$@/release/$@.pdb $(AS_TARGET_BIN)

