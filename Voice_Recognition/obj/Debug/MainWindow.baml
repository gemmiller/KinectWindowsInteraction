   M S B A M L   `   `   `  ����  SpeechBasics-WPF6    0Microsoft.Samples.Kinect.SpeechBasics.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6F   -   5          � Title$  Skeleton Basics��5   	   $	��735��6!   $��798.313��6.   ���	#FF873D3D � 	Resources 5         MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush �� 
TargetType �� Property �� Value  SquareCheckBox �� 
TargetType �� Orientation �Name	 ��Name
 ��Name �� TextAlignment �� Triggers P� Property P� Value �� 
TargetName��%1  &	        &	 #     &	 F     '�� i     &	 �     7  7  �� 5   
   $ 	#ff6e6e6e��62   �� 5	   
   $ 	#ff52318f��64   �� 5
   
   $ 	#ff00BCF2��62   �� 5   
   " ��6   .�5      ��  �@J�SnapsToDevicePixels . 64   5      �� 5   
   " ��6   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      �� $ 
Horizontal=�6)   
������ .+�5   "   � $ SquareCheckBoxChecked��6'   .+�5   &   �� $	 CheckedNormal��6,   $��Images\CheckedNormal.png��6C   $
��None=�6e   $��Center=�6t   �� 5   &   $	 CheckedHover��6,   $��Images\CheckedHover.png��6B   $
��None=�6c   $��Center=�6r   $y�	Collapsed=�6�   5   #   � 5   "   $ SquareCheckBoxUnchecked��6'   $y�	Collapsed=�6H   .+�5   &   �� $	 UncheckedNormal��6,   $ ��Images\UncheckedNormal.png��6E   $
��None=�6i   $��Center=�6x   �� 5   &   $	 UncheckedHover��6,   $��Images\UncheckedHover.png��6D   $
��None=�6g   $��Center=�6v   $y�	Collapsed=�6�   5   #   �� 5   "   $
 SquareCheckBoxText��6,   #��z��6H   $
 Left=�6i   $��Center=�6   8��  $��15(�6�   $��9,0,0,0q�6�   5      5       5      P� 5        �@d�	IsChecked$ falseY�6;   . �5!   "   �� $ SquareCheckBoxChecked��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5"   "   $ SquareCheckBoxUnchecked��6O    � y�$ Visible=�6?   6O   5#      P� 5$       �@J�IsMouseOver . . �5%   "   �� $ CheckedNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5&   "   $ UncheckedNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5'   "   $ CheckedHover��6O    � y�$ Visible=�6?   6O   �� 5(   "   $ UncheckedHover��6O    � y�$ Visible=�6?   6O   �� 5)   "   8  $ SquareCheckBoxText��6h    � ��5*      5,      5.      5/      50      .��52      � -   5        $ 
layoutGrid��52      $��	10 0 10 0q�6   $
��auto��62    � RowDefinitions 53   
   �� 54      $
��Auto �6   �� 55      �� 56      $
��Auto �6   $��75��6*   �� 57      $
��Auto �6   .+�59   
   ]� $��0��6   $��0 0 0 20q�6!   .+�5:      �� $
��Left=�6   $��Images\Logo.png��6*   $
��None=�6C   $��0 10 0 5q�6R   �� 5;      $��Right=�6   $��0 0 -1 0q�6/   $��Bottom=�6A   #���  6\   $��Segoe UI)�6�   $��22(�6�   $
��Bold"�6�   �� 6�   $�Skeleton Basics��6�   6�   �� 5<      $��1��6   $��Images\Status.png��6$   $
��None=�6?   $��Center=�6N   $��30,0,0,5q�6k   5=      ]� 5>   
   $��1��6   $��0 0 0 0q�6!   .+�5?      �� -   5        $	 Image��5?      $
��Left=�6#   $��Images\Status.png��69   �� 5@      $��Right=�6   ���	#FF3C3C3C$	��194��6G   $��Right=�6S   .+�5A      �� $��0,20,0,0q�6   5C      5D      5E      ]� 5F   
   $��2��6   .+�5G      �� $ 
Horizontal=�6   ���	#FFA4A4A4 J� RenderTransformOrigin$ 0.5,0.5&�6I   z�5H      V� 5I      .~�5J      ��  �� ScaleY$ 1Q�6)    �� ScaleX$ 1Q�64   �� 5K       �� AngleY$ 0Q�6(    �� AngleX$ 0Q�63   �� 5L       �� Angle$ 0Q�6*   T� 5M      5N      .+�5P      �� $��20,4,0,0q�6   $��Center=�6/   $��Center=�6L   5R      �� 5S      $��10 0 0 0q�6   $��Center=�6/   .+�5T      �� -   5        $
 skelId��5T       $	��ID:��60   $��Segoe UI)�6;   $��18(�6Q   #���  6_   $��24��6�   $��20 0 0 0q�6�   �� 5U      -   5        $
 txtTracking��5U       $	��ID:��65   $��Segoe UI)�6@   $��18(�6V   #���  6d   $��24��6�   $��20 0 0 0q�6�   �� 5V      -   5        $
 txtIdentified��5V       $	��ID:��67   $��Segoe UI)�6B   $��18(�6X   #���  6f   $��24��6�   $��20 0 0 0q�6�   5W      �� 5X      $��50 0 0 0q�6   $��Center=�6/   .+�5Y      �� -   5        $
 lblClock��5Y       $��Clock:��62   $��Segoe UI)�6@   $��18(�6V   #���  6d   $��24��6�   �� 5Z      -   5        $
 txtPositionX��5Z       $��
Left Hand:��66   $��Segoe UI)�6H   $��18(�6^   #���  6l   $��24��6�   �� 5[      -	   5        $
 txtPositionY��5[       $��Right Hand:��66   $��Segoe UI)�6I   $��18(�6_   #���  6m   $��24��6�   5\      �� 5]      $��50 0 0 0q�6   $��Center=�6/   .+�5^      �� -
   5        $
 txtReachLabel��5^       $��Reach:��67   $��Segoe UI)�6E   $��18(�6[   #���  6i   $��24��6�   �� 5_      -   5        $
 	txtLReach��5_       $��Identified:��63   $��Segoe UI)�6F   $��18(�6\   #���  6j   $��24��6�   �� 5`      -   5        $
 	txtRReach��5`       $��Identified:��63   $��Segoe UI)�6F   $��18(�6\   #���  6j   $��24��6�   5a      �� 5b      $��50 0 0 0q�6   $��Center=�6/   .+�5c      �� -   5        $
 txtTravelLabel��5c       $��Travel:��68   $��Segoe UI)�6G   $��18(�6]   #���  6k   $��24��6�   �� 5d      -   5        $
 
txtLTravel��5d       $��Identified:��64   $��Segoe UI)�6G   $��18(�6]   #���  6k   $��24��6�   �� 5e      -   5        $
 
txtRTravel��5e       $��Identified:��64   $��Segoe UI)�6G   $��18(�6]   #���  6k   $��24��6�   5f      5g      5h      �� 5i   
   -   5         ��Name$ 	statusBar��5i   ?   $��3��6   $��Stretch=�6!   $��Bottom=�6R   
�������#��[  6�   $��26��6�   .@�5j      �� $��0 0 0 10q�6   ���	#FFA4A4A4.��5k      �� -   5        $
 statusBarText��5k      $��10,0,0,0q�63   #���  6E   �� 6w   $6�0Click 'Seated' to change skeletal pipeline type!��6{   6�   5l      5m      5n      5o      