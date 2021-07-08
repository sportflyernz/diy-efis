NAME=Kotuku.Efi

CSC=csc.exe
MDP=../../tools/CanFly.Build.exe
FLAGS=-langversion:1 -nostdlib -target:exe -debug:full
MDPFLAGS=

SRC= \
	Program.cs \
	EFIWidget.cs \
	EFIDialGauge.cs \
	TempWidget.cs \
	SliderGauge.cs \
	Annunciator.cs \
	Marquee.cs \
	Properties/AssemblyInfo.cs

LIBS=-reference:../assemblies/mscorlib.dll;../assemblies/CanFly.Photon.dll;../assemblies/CanFly.Proton.dll
RM =rm -rf

OUTPATH=../assemblies

all: $(OUTPATH)/$(NAME).pe

debug: MDPFLAGS += -d
debug: $(OUTPATH)/$(NAME).pe

release: $(OUTPATH)/$(NAME).pe
release: FLAGS += -optimize

$(OUTPATH)/$(NAME).pe : $(SRC)
	$(CSC) $(FLAGS) $(LIBS) $(SRC) -out:$(OUTPATH)/$(NAME).exe
	$(MDP) -c $(OUTPATH)/$(NAME).exe -p $(OUTPATH)/$(NAME).pe $(MDPFLAGS)

clean:

fclean: clean
	$(RM) $(NAME)

re: fclean all

