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

## Installation Guide

1. Find the [latest release](https://github.com/davidlang42/CARMEN/releases/latest) and download [setup.exe](https://github.com/davidlang42/CARMEN/releases/latest/download/setup.exe) AND [Carmen.msi](https://github.com/davidlang42/CARMEN/releases/latest/download/Carmen.msi)
2. Run setup.exe, and follow the prompts to install CARMEN and any pre-requisites
3. If you receive an error while installing (when your user is not an administrator), try changing the "Install CARMEN for yourself, or anyone who uses this computer" setting to "Just me" on the 2nd screen.
4. Once installed, you can run "CARMEN: Casting And Role Management Equality Network" from the start menu, under the "CARMEN" folder

## Login Guide

If you have been given a username and password for a database server, you should connect as follows;

1. Run "CARMEN: Casting And Role Management Equality Network" from the start menu
2. Click the "Connect" button on the right hand side of the "CARMEN: Show Selection" window
3. In the "Connect to Database" dialog, type the address of the server (eg. example.domain.com) in "Server Host"
4. Type the name of the database (eg. show_2021) in "Database Name"
5. Type your username and password in "Username" and "Password"
6. Click "Connect" to save this connection, and open the show
7. Next time you open CARMEN, you can skip steps 2-6 by selecting the saved connection from the list below the "New"/"Open"/"Connect" buttons

## Basic User Guide

Once you have connected to a database server, or opened a local database file using the "New" or "Open" buttons, the main menu is shown. In the lower left quadrant of this window you will see the list of "steps" in the casting process.

Each step is either:
- ticked in green if complete,
- crossed in red if there are errors,
- or highlighted yellow if they are not yet done.

Under normal circumstances you would work on each of these steps in order, completing each before moving to the next. Clicking a step shows a sub page with the required information and controls to complete the step. A brief description of what you can do on each page is included below.

### Configuring Show
This page allows configuring the various parameters about how YOUR show structure works.

The audition criteria define what you will be marking each applicant on in the audition.

The cast group(s) is what successful applicants will be selected into. For a simple show, this may be a single group called "Cast". Successful applicants can only be in one cast group.

Alternative casts may be used if you use multiple sets of cast members, which alternate performances. Every role must be allocated to one person in each alternative cast.

Cast tags can be applied to any successful applicant (that is, anyone in any cast group) which meet certain criteria. Applicants may have more than one tag.

Section types define the types of structures you will have in your show, for example, Acts or Scenes. Each section type may have different rules about duplicate roles or being cast in consecutive items within that section.

Requirements define what attributes are needed by applicants to be eligible or desirable for a certain cast group, tag or role.

### Registering Applicants
This page allows adding and editing applicant information. It will be marked as complete when enough appicants have been registered to meet the required number of cast members in each cast group, and every appicant has all basic information (name, age, gender) completed.

Applicants may have some audition marks pre-populated at this point, but it is not required for this step to be complete.

### Auditioning
This page is similar to the Registering Applicants page, except that the goal has shifted to entering the applicant's audition marks for each criteria. This will only be marked as complete when every applicant has a mark for all required audition criteria.

### Selecting Cast
This page allows selection of applicants into cast groups, alternative casts and tags. It can be done automatically based on the configured requirements, or manually, or a combination of the two.

It also creates the cast list, which involves assigning each cast member a unique number. If the cast member is in a cast group which uses alternative casts, there will be multiple cast members assigned the same number, one for each alternative cast. This represents that they are the matching cast member in the other alternative cast.

### Configuring Items
This page is where the structure of your show is configured. Starting from the heading of the "show", you can add as many items or sections recursively as you would like. The types of sections available are those configured in "Configuring Show".

This is also where you add the roles in the show, which will be allocated to cast members. Roles can be in 1 or more items, and items can be in any type of section, or directly in the "show". Roles cannot be directly in a section or the "show", therefore the simpliest structure would be a "show" with 1 item, containing 1 or more roles.

### Allocating Roles
This page is where most of the time is spent in the casting process. Depending on the complexity of your show it can take many hours to allocate cast members to all of the roles which meet your various requirements and business logic.

On the left-hand side of this page you will see a list of all the roles in the show, separated by the item/section show structure as defined in "Configuring Items". Each item has either
- a green tick, to indicate that it is fully cast
- a red cross, to indicate that there is an error, or missing cast
- no icon, if the role has no cast allocated yet.

Each section/item can be expanded or collapsed to manage a long list of roles effectively. Below this list is the "Show completed" option which can be unchecked to hide any fully cast roles from the list, leaving only those which still have work to be done visible. There is also an overall progress bar.

Single clicking a role in this list will show the currently allocated cast and requirements of the role on the right-hand side of the screen. Double clicking the role name, the current cast list, or clicking the "Edit casting" button will show a list of available cast members for this role. In this list you can select or deselect cast members to allocate or de-allocate them from this role, and click "Save". There are also buttons to clear the cast for this role, or automatically allocate the most suitable cast.

Additionally, selecting an item or section in the roles list will show which roles within it have not yet been fully cast. There is also a button to allocate balanced cast to all (or a sub-selection) of these outstanding roles, which is both faster than automatically casting individual roles and also better balances the talent between the roles.

This step will be marked as complete when all roles have been cast, and all casting rules and requirements are met.
