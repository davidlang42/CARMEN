# CARMEN
Casting And Role Management Equality System is a software package which assists in the casting of roles in theatrical productions. It has been released open source under the GPLv3 license, with the exception of the research paper [Research.pdf](Research.pdf) which is copyright David Lang (2021).

The research paper is included in this repository as the CARMEN software package was the deliverable of this research project, and may be freely read for interest and further research purposes as long as proper attribution is included in any further published works.

## Abstract from [Research.pdf](Research.pdf)
Community and not-for-profit theatre is a common hobby for all ages, but casting people into roles appropriate for their ability level can be a challenging task, particularly when considering large groups and alternating casts of children.

This project sets out to evaluate the use of SAT solving and neural network based algorithms for the selection of cast and allocation of roles in theatrical productions, and incorporate the successful concepts into a working software solution.

In order to implement the required algorithms, the boolean satisfiability problem (SAT) and artificial neural networks (ANNs) have been researched. A critical review has also been completed of the software packages previously used by Cumberland Gang Show (CGS), a production company which is used as a case study.

The hypotheses of this research are that SAT solving algorithms will improve the balance of talent between alternative casts, and that machine learning neural network models will improve the accuracy of recommendations and spread of roles allocated to applicants, over the previously used heuristic algorithms.

Six SAT based algorithms for cast selection have been implemented and assessed, with five of them found to improve on the heuristic baseline, and four improving on the historical cast selection as performed manually by the production team. This proves that SAT solving algorithms will improve the balance of talent between alternative casts.

Three ANN based algorithms for role allocation have been implemented and assessed, with various parameters totalling 32 neural models. All of these were found to improve accuracy over the heuristic baseline, proving that neural networks will improve the accuracy of recommendations, however the spread of roles was found to be less, disproving that hypothesis. A thorough discussion is included on the reasons why this may have occurred, and future research which could be conducted in this area.

The completed software package, implementing the successful algorithms, has been named “CARMEN: Casting And Role Management Equality Network” and released open source, under the GPLv3 license. The user acceptance testing by five members of the CGS production team found it was faster, easier to use and more intelligent than the previous software, with all five recommending it for use by other production companies.

This deliverable, and the research that went into it, has improved the quality of the casting process of theatrical productions. It will first be used for a real world production by the Cumberland Gang Show in December 2021, as they start auditions for their July 2022 performance, and will help the volunteer production team to share roles fairly amongst applicants, leaving them to focus on what they do best; developing the talent, creativity and passion for the arts of the youth (and youth-at-heart) of Sydney, Australia.
