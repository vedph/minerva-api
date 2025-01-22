# Partitioning Documents

Partitioning TEI files into smaller chunks happens in 2 stages: first we add `milestone` elements with the partition command; then we split files with the split command.

Assuming that we have all the TEI files with their metadata (`.meta`) in an `ac` desktop folder:

(1) partition:

```sh
./an-tool partition-tei C:/Users/dfusi/Desktop/ac *.xml C:/Users/dfusi/Desktop/ac/milestoned
```

(2) split:

```sh
./an-tool split-tei C:/Users/dfusi/Desktop/ac/milestoned *.xml C:/Users/dfusi/Desktop/ac/partitioned -m C:/Users/dfusi/Desktop/ac
```
