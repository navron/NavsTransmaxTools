as_ac: as_lib acsvc 

as_ac_path=ac/as
as_ac_prefix=Tsd.AccessControl.ApplicationServer

aclib: as_setup SoapService
	as/blddll $(as_ac_path)/$@ $@
	scripts/cpy $(as_ac_path)/$@/release/$@.dll $(AS_TARGET_BIN)
	scripts/cpy $(as_ac_path)/$@/release/$@.pdb $(AS_TARGET_BIN)

acsvc: as_setup aclib aslex asmock
	as/bldsvc $(as_ac_path)/$@ $@
	scripts/cpy $(as_ac_path)/$@/release/$@.exe $(AS_TARGET_BIN)
	scripts/cpy $(as_ac_path)/$@/release/$@.pdb $(AS_TARGET_BIN)
