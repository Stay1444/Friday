using System.Globalization;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Common.Entities;
using Friday.UI;
using Friday.UI.Entities; 
using Friday.UI.Extensions;

namespace Friday.Modules.Birthday.Commands;

public class Commands : FridayCommandModule
{
    private BirthdayModule _module;

    public Commands(BirthdayModule module)
    {
        _module = module;
    }

    [Command("birthday")]
    public async Task cmd_BirthdayCommand(CommandContext ctx)
    {
        var uiBuilder = new FridayUIBuilder();
        if (!await _module.DoesUserHaveBirthdayAsync(ctx.User))
        {
            uiBuilder.OnRender(x =>
            {
                x.Embed.Transparent();
                x.Embed.WithTitle("Birthday");
                x.Embed.WithDescription("You don't have a birthday set!");
                x.Embed.WithAuthor(ctx.User.Username, null, ctx.User.AvatarUrl);
                
                x.AddButton(add =>
                {
                   add.Label = "Set Birthday";
                   add.Style = ButtonStyle.Success;
                   add.OnClick(() => x.SubPage = "add");
                });
                
                x.AddSubPage("add", addPage =>
                {
                    var ageVisible = x.GetState("ageVisible", true);
                    var birthday = x.GetState<DateTime?>("age", null);
                    addPage.Embed.Transparent();
                    addPage.Embed.WithTitle("Birthday - Add");
                    addPage.Embed.Description = "Enter your birthday in the following format: \n";
                    addPage.Embed.Description += "```\n";
                    addPage.Embed.Description += "YYYY/MM/DD\n";
                    addPage.Embed.Description += "```";
                    addPage.Embed.Description += "Example: \n";
                    addPage.Embed.Description += "```\n";
                    var paddedMonth = DateTime.UtcNow.Month.ToString().PadLeft(2, '0');
                    var paddedDay = DateTime.UtcNow.Day.ToString().PadLeft(2, '0');
                    addPage.Embed.Description += $"{DateTime.UtcNow.Year}/{paddedMonth}/{paddedDay}\n";
                    addPage.Embed.Description += "```";

                    if (birthday.Value is not null)
                    {
                        // birthday is set to the year the user was born. Calculate how many days until their birthday

                        var bd = new Entities.Birthday(ctx.User.Id, birthday.Value.Value);
                        var nextBirthday = bd.CalculateNextBirthday();
                        var daysRemaining = (nextBirthday - DateTime.UtcNow).TotalDays;
                        addPage.Embed.AddField("Birthday", $"```\n{birthday.Value.Value:yyyy/MM/dd}\n```\n{Math.Round(daysRemaining)} days remaining");
                        
                    }
                    
                    addPage.Embed.AddField(":warning: Warning", "You will **not** be able to change your birthday once set!");


                    addPage.AddButton(ageVisibleBtn =>
                    {
                        ageVisibleBtn.Label = ageVisible.Value ? "Age Public" : "Age Private";
                        ageVisibleBtn.Style = ageVisible.Value ? ButtonStyle.Success : ButtonStyle.Danger;
                        ageVisibleBtn.OnClick(() =>
                        {
                            ageVisible.Value = !ageVisible.Value;
                        });
                    });
                    
                    addPage.AddModal(modal =>
                    {
                        modal.Title = "Birthday";
                        modal.ButtonLabel = "Set Birthday";
                        modal.ButtonStyle = ButtonStyle.Primary;
                        modal.ButtonEmoji = DiscordEmoji.FromName(ctx.Client, ":pencil2:");
                        
                        modal.AddField("date", field =>
                        {
                            field.Placeholder = "YYYY/MM/DD";
                            field.Required = true;
                            field.MinimumLength = 10;
                            field.MaximumLength = 10;
                            field.Style = TextInputStyle.Short;
                            field.Title = "Date";
                            if (birthday.Value is not null)
                            {
                                field.Value = birthday.Value.Value.ToString("yyyy/MM/dd");
                            }
                        });

                        modal.OnSubmit(fields =>
                        {
                            if (fields.IsEmpty()) return;
                            
                            var dateString = fields["date"];

                            if (!DateTime.TryParseExact(dateString,
                                    "yyyy/MM/dd",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out var date))
                            {
                                return;
                            }
                            
                            if (date < new DateTime(1900, 1, 1)) return;
                            if (date > DateTime.UtcNow) return;
                            
                            birthday.Value = date;

                            x.ForceRender();
                        });
                    });

                    if (birthday.Value is not null)
                    {
                        addPage.AddButton(confirm =>
                        {
                            confirm.Label = "Confirm";
                            confirm.Emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                            confirm.Style = ButtonStyle.Success;
                            confirm.OnClick(async () =>
                            {
                                await _module.DatabaseService.InsertBirthdayAsync(
                                    new Entities.Birthday(ctx.User.Id, birthday.Value.Value));
                                x.OnCancelledAsync((_, msg) => msg.DeleteAsync());
                                x.Stop();
                            });
                        });
                    }
                });
            });

            await ctx.SendUIAsync(uiBuilder);
            return;
        }
    }
}