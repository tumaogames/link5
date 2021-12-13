@implementation MyPlugin
-(void) ShareMatchLink: (const char *) shareMessage
{
    
    NSString *message   = [NSString stringWithUTF8String:shareMessage];
    NSArray *postItems  = @[message];
    
    UIActivityViewController *activityVc = [[UIActivityViewController alloc] initWithActivityItems:postItems applicationActivities:nil];

       activityViewController.completionWithItemsHandler = ^(NSString *activityType, BOOL completed,
                                                          NSArray *returnedItems, NSError *activityError){
        NSLog(@"Activity %@ completed: %d",activityType, completed);
        if (activityError!=nil)
            NSLog(@"Error: %@",[activityError localizedDescription]);
        if (shareCallBack!=nil)
            shareCallBack(completed);
    
    
    if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPhone )
    {
            [
                UnityGetGLViewController() presentViewController:activityViewController animated:YES completion:^{
                NSLog(@"share presented");
            }];
    }
    else 
    {
        UIPopoverController *popover = [[UIPopoverController alloc] initWithContentViewController:activityViewController];
        UIView *mainView = UnityGetGLView();
        popover.delegate = nil;
        [   
            popover presentPopoverFromRect:
            CGRectMake(mainView.frame.size.width/2, mainView.frame.size.height-10, 0, 0)
            inView:mainView
            permittedArrowDirections:UIPopoverArrowDirectionAny
            animated:YES
        ];
	}
        
}
@end

// Globally declare text sharing method
extern "C" {
    void _TAG_ShareSimpleText(const char * message){
        MyPlugin *vc = [[MyPlugin alloc] init];
        [vc ShareMatchLink: message];
    }
}