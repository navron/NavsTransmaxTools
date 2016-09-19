test_one: one two

test_one_path=ac/as
test_one_prefix=Tsd.core.one

one: other two
	as/blddll $(test_one_path)/$@ $@
	scripts/cpy $(test_one_path)/$@/release/$@.dll $(AS_TARGET_BIN)
	scripts/cpy $(test_one_path)/$@/release/$@.pdb $(AS_TARGET_BIN)

two: other
	as/bldsvc $(test_one_path)/$@ $@
	scripts/cpy $(test_one_path)/$@/release/$@.exe $(AS_TARGET_BIN)
	scripts/cpy $(test_one_path)/$@/release/$@.pdb $(AS_TARGET_BIN)
