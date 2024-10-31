# Partitioning Documents

Partitioning TEI files into smaller chunks happens in 2 stages: first we add `milestone` elements with the partition command; then we split files with the split command.

Assuming that we have all the original TEI files with their metadata (`.meta`) in a `blob` desktop folder:

(1) partition:

```ps1
./an-tool partition-tei C:\Users\dfusi\Desktop\blob\full\fi *.xml C:\Users\dfusi\Desktop\blob\temp\fi\
./an-tool partition-tei C:\Users\dfusi\Desktop\blob\full\ge *.xml C:\Users\dfusi\Desktop\blob\temp\ge\
./an-tool partition-tei C:\Users\dfusi\Desktop\blob\full\le *.xml C:\Users\dfusi\Desktop\blob\temp\le\
./an-tool partition-tei C:\Users\dfusi\Desktop\blob\full\vt *.xml C:\Users\dfusi\Desktop\blob\temp\vt\
xcopy C:\Users\dfusi\Desktop\blob\full\fi\*.meta C:\Users\dfusi\Desktop\blob\temp\fi\
xcopy C:\Users\dfusi\Desktop\blob\full\ge\*.meta C:\Users\dfusi\Desktop\blob\temp\ge\
xcopy C:\Users\dfusi\Desktop\blob\full\le\*.meta C:\Users\dfusi\Desktop\blob\temp\le\
xcopy C:\Users\dfusi\Desktop\blob\full\vt\*.meta C:\Users\dfusi\Desktop\blob\temp\vt\
```

(2) split:

```ps1
./an-tool split-tei C:\Users\dfusi\Desktop\blob\temp\fi *.xml C:\Users\dfusi\Desktop\blob\part\fi\
./an-tool split-tei C:\Users\dfusi\Desktop\blob\temp\ge *.xml C:\Users\dfusi\Desktop\blob\part\ge\
./an-tool split-tei C:\Users\dfusi\Desktop\blob\temp\le *.xml C:\Users\dfusi\Desktop\blob\part\le\
./an-tool split-tei C:\Users\dfusi\Desktop\blob\temp\vt *.xml C:\Users\dfusi\Desktop\blob\part\vt\
```
