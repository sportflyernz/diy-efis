NAME=System.Collections

CSC=csc.exe
MDP=../../tools/CanFly.Build.exe
FLAGS=-langversion:1 -nostdlib -target:library -debug:full
MDPFLAGS=

SRC= \
	DictionaryEntry.cs \
	Hashtable.cs \
	IDictionary.cs \
	Queue.cs \
	Stack.cs \
	Properties/AssemblyInfo.cs

LIBS=-reference:../assemblies/mscorlib.dll
RM =rm -rf

OUTPATH=../assemblies

all: $(OUTPATH)/$(NAME).pe

debug: MDPFLAGS += -d
debug: $(OUTPATH)/$(NAME).pe

release: $(OUTPATH)/$(NAME).pe
release: FLAGS += -optimize

$(OUTPATH)/$(NAME).pe : $(SRC)
	$(CSC) $(FLAGS) $(LIBS) $(SRC) -out:$(OUTPATH)/$(NAME).dll
	$(MDP) -c $(OUTPATH)/$(NAME).dll -p $(OUTPATH)/$(NAME).pe  $(MDPFLAGS)

clean:

fclean: clean
	$(RM) $(NAME)

re: fclean all

